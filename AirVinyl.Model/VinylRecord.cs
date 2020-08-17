using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AirVinyl.Model
{
    public class VinylRecord
    {
        private IDictionary<string, object> _properties;

        public VinylRecord()
        {
            DynamicVinylRecordProperties = new List<DynamicProperty>();
        }

        [Key] public int VinylRecordId { get; set; }

        [StringLength(150)] [Required] public string Title { get; set; }

        [StringLength(150)] [Required] public string Artist { get; set; }

        [StringLength(50)] public string CatalogNumber { get; set; }

        public int? Year { get; set; }

        public PressingDetail PressingDetail { get; set; }

        public virtual Person Person { get; set; }

        public IDictionary<string, object> Properties
        {
            get
            {
                if (_properties == null)
                {
                    _properties = new Dictionary<string, object>();
                    foreach (var dynamicProperty in DynamicVinylRecordProperties)
                        _properties.Add(dynamicProperty.Key, dynamicProperty.Value);
                }

                return _properties;
            }
            set => _properties = value;
        }

        public ICollection<DynamicProperty> DynamicVinylRecordProperties { get; set; }
    }
}