using System.Collections.Generic;

namespace FruitMod.Economy
{
    public class Store
    {
        public enum Shop
        {
            Moms_Garden,
            Small_Factory,
            Large_Factory,
            Farmers,
            Plant_Whisperers,
            Sugar_Field,
            Cooking_Mama,
            Fertilizer,
            Sprinklers,
            Guy_Fieri,
        }

        public static Dictionary<Shop, int> shopModifiers = new Dictionary<Shop, int>
        {
            [Shop.Moms_Garden] = 2,
            [Shop.Small_Factory] = 3,
            [Shop.Large_Factory] = 4,
            [Shop.Farmers] = 2,
            [Shop.Plant_Whisperers] = 2,
            [Shop.Sugar_Field] = 4,
            [Shop.Cooking_Mama] = 4,
            [Shop.Fertilizer] = 1,
            [Shop.Sprinklers] = 1,
            [Shop.Guy_Fieri] = 4
        };

        public static Dictionary<Shop, int> shopPrices= new Dictionary<Shop, int>
        {
            [Shop.Moms_Garden] = 5000,
            [Shop.Small_Factory] = 10000,
            [Shop.Large_Factory] = 15000,
            [Shop.Farmers] = 500,
            [Shop.Plant_Whisperers] = 500,
            [Shop.Sugar_Field] = 5000,
            [Shop.Cooking_Mama] = 5000,
            [Shop.Fertilizer] = 250,
            [Shop.Sprinklers] = 250,
            [Shop.Guy_Fieri] = 5000
        };
    }
}