using LagoVista.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.Client.Core.Models
{
    public class HelpRequest
    {
        public enum HelpRequestTypes
        {
            Requirements,
            SettiingsConfiguration,
            CreatingNewAlgorithm,
            UnderstandingExistingAlgorithm,
            Debugging,
            UserInterface,
            NewTechnology,
            Permissions,
        }

        public enum HelpRequestPriority
        {
            Critical,
            High,
            Medium,
            Low,
        }

        public HelpRequest(string workTaskId)
        {
            _workTaskId = workTaskId;
        }

        private readonly string _workTaskId;

        public const string RequestType_Requirements = "requirements";
        public const string RequestType_SettingsConfiguration = "settings";

        public const string Priority_Critical = "critical";
        public const string Priority_High = "high";

        public string Id { get; set; }

        public string Name { get; set; }

        public string CreationData { get; set; }

        public string ModificationDate { get; set; }

        public EntityHeader<HelpRequestTypes> RequestType { get; set; }
        public EntityHeader<HelpRequestPriority> Priority { get; set; }

        public EntityHeader WhoNeedsHelp { get; set; }

        public EntityHeader WhoCanHelp { get; set; }

        public string ProblemDescription { get; set; }

        public string WhatHaveYouTried { get; set; }
        public string WhatDidYouLookFor { get; set; }
    }
}
