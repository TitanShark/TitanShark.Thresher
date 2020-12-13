using System;
using System.Collections.Generic;

namespace TitanShark.Thresher.Core
{
    public class CallId : IEqualityComparer<CallId>
    {
        public string Id { get; set; }

        public DateTime UtcCreated { get; set; }

        public static CallId CreateFromGuid()
        {
            return new CallId
            {
                Id = Guid.NewGuid().ToString("N"),
                UtcCreated = DateTime.UtcNow
            };
        }

        public override bool Equals(object obj)
        {
            var target = obj as CallId;

            if (target == null)
            {
                return false;
            }

            return string.Equals(Id, target.Id, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public bool Equals(CallId x, CallId y)
        {
            if (x == null && y == null)
            {
                return true;
            }

            if ((x != null && y == null) || (x == null && y != null))
            {
                return false;
            }

            return x.Equals(y);
        }

        public int GetHashCode(CallId obj)
        {
            return obj.GetHashCode();
        }
    }
}
