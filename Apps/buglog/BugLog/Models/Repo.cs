using LagoVista.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace BugLog.Models
{
    public class Repo : ModelBase
    {
        public string Id { get; set; }

        public string ProjectId { get; set; }

        private string _name;
        public string Name
        {
            get => _name;
            set => Set(ref _name, value);
        }

        public string _devBranch;
        public string DevBranch
        {
            get => _devBranch;
            set => Set(ref _devBranch, value);
        }

        public string _testBranch;
        public string TestBranch
        {
            get => _testBranch;
            set => Set(ref _testBranch, value);
        }

        private string _folder;
        public string Folder
        {
            get => _folder;
            set => Set(ref _folder, value);
        }
    }
}
