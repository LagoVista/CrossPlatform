using LagoVista.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.XPlat.Sample
{
    public class Model2 : IEntityHeaderEntity
    {
        public string Id { get; set; }

        public IEntityHeader ToEntityHeader()
        {
            throw new NotImplementedException();
        }
    }
}