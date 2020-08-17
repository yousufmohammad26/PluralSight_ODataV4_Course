using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace AirVinyl.Model
{
    public class DynamicProperty
    {
        [Key] [Column(Order = 1)] public string Key { get; set; }

        [Key] [Column(Order = 2)] public int VinylRecordId { get; set; }

        public string SerializedValue { get; set; }

        public object Value
        {
            get => JsonConvert.DeserializeObject(SerializedValue);
            set => SerializedValue = JsonConvert.SerializeObject(value);
        }

        public virtual VinylRecord VinylRecord { get; set; }
    }
}