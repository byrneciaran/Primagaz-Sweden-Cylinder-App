namespace Primagaz.Android
{
    public static class ProductIcon
    {
        public static int? GetIcon(string productCode)
        {

            switch (productCode)
            {
                case "11100":
                    return Resource.Drawable.product_11100;
                case "11102":
                    return Resource.Drawable.product_11102;
                case "11206":
                    return Resource.Drawable.product_11206;
                case "11211":
                    return Resource.Drawable.product_11211;
                case "11219":
                    return Resource.Drawable.product_11219;
                case "11245":
                    return Resource.Drawable.product_11245;
                case "11260":
                    return Resource.Drawable.product_11260;
                case "11311":
                    return Resource.Drawable.product_11311;
                case "11416":
                    return Resource.Drawable.product_11416;
                case "11460":
                    return Resource.Drawable.product_11460;
                case "1189535":
                    return Resource.Drawable.product_1189535;
                case "1189536":
                    return Resource.Drawable.product_1189536;
                case "12206":
                    return Resource.Drawable.product_12206;
                case "12211":
                    return Resource.Drawable.product_12211;
                case "13205":
                    return Resource.Drawable.product_13205;
                case "13210":
                    return Resource.Drawable.product_13210;
                case "14103":
                    return Resource.Drawable.product_14103;
                case "999":
                    return Resource.Drawable.product_999;
                default:
                    return null;
            }

        }
    }
}
