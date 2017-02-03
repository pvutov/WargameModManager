using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WargameModManager {
    public class ModUpdateInfo {
        private Version version;
        private String url;
        private String modDir;
        private String name;

        public ModUpdateInfo(String name, String version, String url, String dir) {
            this.name = name;
            modDir = dir;
            this.version = new Version(version);
            this.url = url;
        }

        public Version getVersion() {
            return version;
        }

        public String getUrl() {
            return url;
        }

        public String getModDir() {
            return modDir;
        }

        public String getName() {
            return name;
        }
    }
}
