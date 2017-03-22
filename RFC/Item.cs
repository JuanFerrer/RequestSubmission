using System.ComponentModel;

namespace RFC
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum EPriority
    {
        [Description("High")]
        HIGH,
        [Description("Normal")]
        NORMAL,
        [Description("Low")]
        LOW
    }

    public class Item
    {

        public string Author { get; set; }
        public string Request { get; set; }
        public EPriority Priority { get; set; }

        public Item(string author, string req, EPriority priority)
        {
            Author = author;
            Request = req;
            Priority = priority;
        }
    }
}
