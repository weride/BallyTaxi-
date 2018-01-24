namespace Quickode.BallyTaxi.Models
{
    public partial class Image
    {
        public string Filename
        {
            get
            {
                return ImageId.ToString() + "." + Extension;
            }
        }
    }
}