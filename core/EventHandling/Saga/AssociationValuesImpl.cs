using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using NAxonFramework.Common;

namespace NAxonFramework.EventHandling.Saga
{
    public class AssociationValuesImpl : IAssociationValues
    {
        private readonly ISet<AssociationValue> _values = new HashSet<AssociationValue>();
        private readonly ISet<AssociationValue> _addedValues = new HashSet<AssociationValue>();
        private readonly ISet<AssociationValue> _removedValues = new HashSet<AssociationValue>();
        public AssociationValuesImpl(ISet<AssociationValue> associationValues)
        {
            associationValues.ForEach(x => _values.Add(x));
        }

        public int Size => _values.Count;

        public bool Contains(AssociationValue associatedValue) => _values.Contains(associatedValue);

        public IEnumerator<AssociationValue> GetEnumerator() => _values.ToList().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool Add(AssociationValue associationValue)
        {
            var added = _values.Add(associationValue);
            if (added) 
            {
                if (!_removedValues.Remove(associationValue)) 
                {
                    _addedValues.Add(associationValue);
                }
            }
            return added;
        }

        public bool Remove(AssociationValue associationValue)
        {
            var removed = _values.Remove(associationValue);
            if (removed) 
            {
                if (!_addedValues.Remove(associationValue)) 
                {
                    _removedValues.Add(associationValue);
                }
            }
            return removed;
        }

        public ISet<AssociationValue> AsSet()
        {
            return _values.ToHashSet();
        }

        public ISet<AssociationValue> RemovedAssociations
        {
            get
            {
                if (_removedValues.IsEmpty())
                {
                    return new HashSet<AssociationValue>();
                }

                return _removedValues;
            }
        }

        public ISet<AssociationValue> AddedAssociations
        {
            get
            {
                if (_addedValues.IsEmpty())
                {
                    return new HashSet<AssociationValue>();
                }

                return _addedValues;
            }
        }

        public void Commit()
        {
            _addedValues.Clear();
            _removedValues.Clear();
        }
    }
}