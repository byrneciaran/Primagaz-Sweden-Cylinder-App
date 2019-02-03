namespace Primagaz.Android
{
    public static class ProductIcon
    {
        public static int? GetIcon(string productCode)
        {

            switch (productCode)
            {
                case "1001":
                    return Resource.Drawable.cylinder_kugle2Kg;
                case "182586":
                    return Resource.Drawable.ruko_3641;
                case "1111":
                case "1105":
                    return Resource.Drawable.cylinder_tysk11kg;
                case "1006":
                    return Resource.Drawable.cylinder_alu6kg;
                case "1002":
                    return Resource.Drawable.cylinder_co2kg;
                case "1013":
                    return Resource.Drawable.cylinder_alu11kg;
                case "1895480":
                    return Resource.Drawable.lan_vogn_tyv;
                case "1895471":
                    return Resource.Drawable.vogn_s18;
                case "1895478":
                    return Resource.Drawable.vogn_s20;
                case "182587":
                    return Resource.Drawable.cylinder_key;
                case "1210":
                case "1213":
                    return Resource.Drawable.cylinder_prima10kg;
                case "1403":
                    return Resource.Drawable.cylinder_cgi3kg;
                case "1402":
                    return Resource.Drawable.cylinder_cgi2kg;
                case "1305":
                    return Resource.Drawable.cylinder_ragasco5kg;
                case "1310":
                    return Resource.Drawable.cylinder_ragasco10kg;
                case "1510":
                    return Resource.Drawable.cylinder_light10kg;
                case "1005":
                    return Resource.Drawable.cylinder_5kg;
                case "1011":
                    return Resource.Drawable.cylinder_11kg;
                case "1017":
                    return Resource.Drawable.cylinder_17kg;
                case "1022":
                    return Resource.Drawable.cylinder_22kg;
                case "1033":
                case "1030":
                    return Resource.Drawable.cylinder_33kg;
                case "189692":
                    return Resource.Drawable.palle_33;
                case "189633":
                    return Resource.Drawable.lasebojle;
                case "189613":
                    return Resource.Drawable.palle_13;
                case "189611":
                    return Resource.Drawable.palle_11;
                case "189605":
                    return Resource.Drawable.palle_5;
                default:
                    return null;
            }

        }
    }
}
