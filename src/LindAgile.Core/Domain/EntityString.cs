using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LindAgile.Core.Domain
{
    public class EntityString : EntityBase, IEntity<string>
    {
        public EntityString()
        {
            this.Id = PrimaryKey.GenerateNewId().ToString();
        }
        public string Id
        {
            get; set;
        }

        public bool Equals(string other)
        {
            return Id.Equals(other);
        }
    }
}
