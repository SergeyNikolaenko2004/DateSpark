namespace DateSpark.API.Models
{
    public enum PriceCategory
    {
        Low = 1,     // $ - дешево/бесплатно
        Medium = 2,  // $$ - средняя цена  
        High = 3     // $$$ - дорого
    }
    
    public static class PriceCategoryExtensions
    {
        public static string ToSymbol(this PriceCategory category)
        {
            return category switch
            {
                PriceCategory.Low => "$",
                PriceCategory.Medium => "$$",
                PriceCategory.High => "$$$",
                _ => "$$"
            };
        }
        
        public static string ToDisplayName(this PriceCategory category)
        {
            return category switch
            {
                PriceCategory.Low => "Дешево/бесплатно",
                PriceCategory.Medium => "Средняя цена",
                PriceCategory.High => "Дорого",
                _ => "Средняя цена"
            };
        }
        
        public static PriceCategory FromSymbol(string symbol)
        {
            return symbol switch
            {
                "$" => PriceCategory.Low,
                "$$" => PriceCategory.Medium,
                "$$$" => PriceCategory.High,
                _ => PriceCategory.Medium
            };
        }
    }
}