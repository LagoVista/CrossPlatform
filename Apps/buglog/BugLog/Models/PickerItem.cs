using System;
using System.Collections.Generic;
using System.Text;

namespace BugLog.Models
{
    public class PickerItem
    {
        public static PickerItem Create(string id, string text)
        {
            return new PickerItem()
            {
                Id = id,
                Text = text,
            };
        }

        public static PickerItem All()
        {
            return new PickerItem()
            {
                Id = "all",
                Text = "All",
                IsDefault = true,
            };
        }


        public string Id { get; set; }
        public string Text { get; set; }
        public bool IsDefault { get; set; }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if(obj is PickerItem pi)
            {
                return pi.Id == Id;
            }

            return false;
        }
    }
}
