using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;
using Windows.Data.Xml.Dom;
using Windows.Storage;

namespace MagicMirror.Utilities
{
    static public class CredintialStore
    {
        static private Dictionary<string, ApplicationCredintials> store = null;
        static private object storeLock = new object();

        private const string settingsFile = @"CredintialStore.xml";

        static public ApplicationCredintials GetCredintials(string application)
        {
            lock (storeLock)
            {
                if (store == null)
                    LoadStore();

                return store[application];
            }
        }

        static public void LoadStore()
        {
            Task.Run(async () =>
            {
                try
                {
                    Uri uri = null;
                    try
                    {
                        var folder = Windows.Storage.KnownFolders.MusicLibrary;
                        var file = await folder.GetFileAsync(settingsFile);
                        uri = new Uri(file.Path);
                    }
                    catch
                    {
                        // Support running on the local computer under "build folder"\AppX\Properties
                        var folder = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync("Properties");
                        var file = await folder.GetFileAsync(settingsFile);
                        uri = new Uri(file.Path);
                    }
                    XDocument xDoc = XDocument.Load(uri.ToString());

                    store = new Dictionary<string, ApplicationCredintials>();

                    foreach (var app in xDoc.Descendants("application"))
                    {
                        store.Add(app.Attribute("name").Value, new ApplicationCredintials { ID = app.Element("id").Value, Secret = app.Element("secret").Value });
                    }
                }
                catch (Exception)
                {
                }

            }).Wait();
        }
    }

    public class ApplicationCredintials
    {
        public string ID { get; set; }
        public string Secret { get; set; }
    }

    public class ApplicationContainer
    {
        [XmlAttribute]
        public string name;
        [XmlText]
        public ApplicationCredintials Credintials;
    }
}
