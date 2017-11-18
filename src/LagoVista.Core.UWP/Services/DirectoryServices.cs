using LagoVista.Core.Models;
using LagoVista.Core.PlatformSupport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.Core.UWP.Services
{
    class DirectoryServices : IDirectoryServices
    {
        public List<Folder> GetFolders(String root)
        {
            var dirStrings = System.IO.Directory.GetDirectories(root);
            var dirs = new List<Folder>();
            foreach(var dir in dirStrings)
            {
                dirs.Add(new Folder() { FullPath = dir });
            }

            return dirs;
        }

        public List<File> GetFiles(String directory)
        {
            var fileStrings = System.IO.Directory.GetFiles(directory);

            var files = new List<File>();
            foreach(var file in fileStrings)
            {
                files.Add(new File() { Directory = directory, FileName = file });
            }

            return files;
        }
    }
}
