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

        private string _folder;
        public string Folder
        {
            get => _folder;
            set => Set(ref _folder, value);
        }
    }
}
