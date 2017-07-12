using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LindAgile.Core.Domain
{
    /// <summary>
    /// 实体基类－会被持久化
    /// 当两个实体的主键相同时，我们认为它是一个实体
    /// </summary>
    [Serializable]
    public abstract class Entity : EntityBase, IEntity<int>
    {
        [DisplayName("编号"), Column("ID"), Required, Key]
        public int Id { get; set; }

        public bool Equals(int other)
        {
            return Id.Equals(other);
        }

        #region override Methods
        public override bool Equals(object obj)
        {
            Console.WriteLine("Equals()");
            if (obj == null || !(obj is Entity))
                return false;

            if (Object.ReferenceEquals(this, obj))
                return true;

            if (this.GetType() != obj.GetType())
                return false;

            Entity item = (Entity)obj;
            if (item.Id == default(int) || this.Id == default(int))
                return false;
            else
                return item.Id == this.Id;
        }

        public override int GetHashCode()
        {
            Console.WriteLine("GetHashCode()");
            if (this.Id != default(int))
                return this.Id.GetHashCode() ^ 31; // XOR for random distribution (http://blogs.msdn.com/b/ericlippert/archive/2011/02/28/guidelines-and-rules-for-gethashcode.aspx)
            else
                return base.GetHashCode();
        }

        public static bool operator ==(Entity left, Entity right)
        {
            if (Object.Equals(left, null))
                return (Object.Equals(right, null)) ? true : false;
            else
                return left.Equals(right);
        }

        public static bool operator !=(Entity left, Entity right)
        {
            return !(left == right);
        }
        #endregion

    }
}

