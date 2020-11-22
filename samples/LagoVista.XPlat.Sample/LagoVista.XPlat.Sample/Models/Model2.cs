using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.XPlat.Sample.Models
{
    public class Model2 : IEntityHeaderEntity
    {
        public string Id { get; set; }

        public string Text { get; set; }

        public IEntityHeader ToEntityHeader()
        {
            return new EntityHeader() { Id = Id, Text = Text };
        }
    }
}