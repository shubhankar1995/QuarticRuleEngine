using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO; 

namespace Quartic_Rule_Engine
{
    class Program
    {
        public static string RULES_LOCATION = Directory.GetCurrentDirectory() + @"/../../Rules.csv";
        public static string DATA_STORE_LOCATION = Directory.GetCurrentDirectory() + @"/../../SignalData.csv";
        public static string JSON_RAW_DATA_LOCATION = Directory.GetCurrentDirectory() + @"/../../raw_data.json";

        static void Main(string[] args)
        {

            string jsonData = getFileContent(JSON_RAW_DATA_LOCATION);

            List<Item> items = getJSONObjects(jsonData);

            for (int i = 0; i < items.Count; i++)
            {
                bool rule_result = ruleValidations(items[i]);

                bool duplicate = isDuplicate(items[i]);

                if (rule_result && !duplicate)
                {
                    saveData(items[i]);
                }

            }

            Console.WriteLine("Press any key to exit.");
            System.Console.ReadKey();

        }


        static string getFileContent(string path)
        {

            string fileData = File.ReadAllText(path);

            return fileData;

        }

        static List<Item> getJSONObjects(string jsonData)
        {

            List<Item> items = JsonConvert.DeserializeObject<List<Item>>(jsonData);

            return items;
        }

        static Boolean ruleValidations(Item item)
        {

            string[] valueTypes = {"INTEGER","STRING","DATETIME"}; 

            if (valueTypes.Contains(item.value_type.ToUpper()))
            {

                bool rulesResult = getFilteredRules(item.signal, item.value_type, item.value);

                return rulesResult;

            }
            else
            {
                Console.WriteLine("Invalid value type in the signal data");
                return false;
            }

        }

        static void saveData( Item item)
        {
            try
            {
                string oldRulesdata = getOldRules();
                string data = "";

                if (oldRulesdata == "" || oldRulesdata == "\r\n")
                {
                    oldRulesdata = "Signal,Value,Value Type\r\n";
                }

                data =oldRulesdata + item.signal + "," + item.value + "," + item.value_type + "\r\n";

                File.WriteAllText(DATA_STORE_LOCATION, data);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error while saving the Data");
            }

            
        }

        static string getOldRules()
        {
            string oldRulesData = "";

            try
            {
                oldRulesData = File.ReadAllText(DATA_STORE_LOCATION);
            }
            catch (Exception ex)
            {
                return "";
            }
            
            return oldRulesData;
          
        }

        static bool isDuplicate(Item item)
        {
            try
            {
                string[] oldRulesData = File.ReadAllLines(DATA_STORE_LOCATION);

                string newRule = item.signal + "," + item.value + "," + item.value_type;

                if (oldRulesData.Contains(newRule))
                {
                    Console.WriteLine("Duplicate Data so not saved in the file, Signal: {0}, Value: {1}, Value Type: {2}", item.signal, item.value, item.value_type);
                    return true;
                }

            }
            catch (Exception ex)
            {
                return false;
            }

            return false;
        }

        static List<Rules> getRules()
        {
            string[] rules = File.ReadAllLines(RULES_LOCATION);

            List<Rules> rulesList= new List<Rules>();

            for (int i = 0; i < rules.Count(); i++)
            {
                Rules rule = new Rules();

                string[] data = rules[i].Split(',');
                rule.signal = data[0].ToString();
                rule.value = data[1].ToString();
                rule.value_type = data[2].ToString();
                rule.operation = data[3].ToString();

                rulesList.Add(rule);

            }

            return rulesList;

        }

        static bool getFilteredRules(string signal, string valueType, string value){

            List<Rules> rulesList = getRules();

            IEnumerable<Rules> ruleQuery = from rule in rulesList where rule.signal.ToUpper() == signal.ToUpper() && rule.value_type.ToUpper() == valueType.ToUpper() select rule;

            foreach (Rules rule in ruleQuery)
            {
                if (valueType.ToUpper() == "INTEGER")
                {
                    if (rule.operation == "NOT MORE THAN" && Convert.ToDouble(value) > Convert.ToDouble(rule.value))
                    {
                        Console.WriteLine("RULE => Value of {0} cannot be greater than {1}, Signal: {2}, Value: {3}, Datatype: {4}", signal, rule.value, signal, value, valueType);
                        return false;
                    }
                    if (rule.operation == "NOT LESS THAN" && Convert.ToDouble(value) < Convert.ToDouble(rule.value))
                    {
                        Console.WriteLine("RULE => Value of {0} cannot be less than {1}, Signal: {2}, Value: {3}, Datatype: {4}", signal, rule.value, signal, value, valueType);
                        return false;
                    }
                    if (rule.operation == "NOT EQUAL TO" && Convert.ToDouble(value) == Convert.ToDouble(rule.value))
                    {
                        Console.WriteLine("RULE => Value of {0} cannot be equal to {1}, Signal: {2}, Value: {3}, Datatype: {4}", signal, rule.value, signal, value, valueType);
                        return false;
                    }
                }
                else if (valueType.ToUpper() == "STRING")
                {
                    if (rule.operation == "NOT EQUAL TO" && value.ToUpper() == rule.value.ToUpper())
                    {
                        Console.WriteLine("RULE => Value of {0} cannot be {1}, Signal: {2}, Value: {3}, Datatype: {4}", signal, rule.value, signal, value, valueType);
                        return false;
                    }
                    if (rule.operation == "EQUAL TO" && value.ToUpper() == rule.value.ToUpper())
                    {
                        Console.WriteLine("RULE => Value of {0} should be {1}, Signal: {2}, Value: {3}, Datatype: {4}", signal, rule.value, signal, value, valueType);
                        return false;
                    }
                }
                else
                {
                    if (rule.operation == "NOT EQUAL TO" && rule.value.ToUpper() == "FUTURE" && Convert.ToDateTime(value) > DateTime.Today)
                    {
                        Console.WriteLine("RULE => Value of {0} cannot be in future, Signal: {1}, Value: {2}, Datatype: {3}", signal, signal, value, valueType);
                        return false;
                    }
                    if (rule.operation == "EQUAL TO" && rule.value.ToUpper() == "FUTURE" && Convert.ToDateTime(value) < DateTime.Today)
                    {
                        Console.WriteLine("RULE => Value of {0} should be in future, Signal: {1}, Value: {2}, Datatype: {3}", signal, signal, value, valueType);
                        return false;
                    }
                    if (rule.operation == "NOT EQUAL TO" && rule.value.ToUpper() == "PAST" && Convert.ToDateTime(value) < DateTime.Today)
                    {
                        Console.WriteLine("RULE => Value of {0} cannot be in past, Signal: {1}, Value: {2}, Datatype: {3}", signal, signal, value, valueType);
                        return false;
                    }
                    if (rule.operation == "EQUAL TO" && rule.value.ToUpper() == "PAST" && Convert.ToDateTime(value) > DateTime.Today)
                    {
                        Console.WriteLine("RULE => Value of {0} should be in past, Signal: {1}, Value: {2}, Datatype: {3}", signal, signal, value, valueType);
                        return false;
                    }
                    if (rule.operation == "NOT EQUAL TO" && rule.value.ToUpper() == "TODAY" && Convert.ToDateTime(value) != DateTime.Today)
                    {
                        Console.WriteLine("RULE => Value of {0} cannot be today, Signal: {1}, Value: {2}, Datatype: {3}", signal, signal, value, valueType);
                        return false;
                    }
                    if (rule.operation == "EQUAL TO" && rule.value.ToUpper() == "TODAY" && Convert.ToDateTime(value) == DateTime.Today)
                    {
                        Console.WriteLine("RULE => Value of {0} should be today, Signal: {1}, Value: {2}, Datatype: {3}", signal, signal, value, valueType);
                        return false;
                    }

                }

            }

            return true;
        }
    }
}
