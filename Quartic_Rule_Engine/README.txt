Briefly describe the conceptual approach you chose! What are the trade-offs?
I started off with deserializing the JSON data into a list as they have a lot of features associated with it. The signal data that was extracted from the JSON is checked with the Rules which have been stored in a CSV file. The rules which are extracted from the CSV file are filtered using linq based on the signal and the data type. The data if satisfies all the rules is then stored into a CSV file.

I have used CSV files instead of a database to keep the dependencies to a minimum.

What's the runtime performance? What is the complexity? Where are the bottlenecks?
The code has a very good runtime performance and can process all the records in the sample JSON file in 1 second. The code has O(2) complexity. When a huge number of rules on the same Signal and data type might cause a marginal effect on the performance.

If you had more time, what improvements would you make, and in what order of priority?
Below are the improvements that I could make in the order of priority.
	1) Make the user enter the path for the JSON file instead of forcing it
	2) Provide header to the rules CSV
	3) Create test cases to test all individual functionalities
	3) Improve resource handling while dealing with the file system
	4) Improve the rule validation logic and use a mapping logic
	5) Try to use natural language processing to get the rules
	6) Make use of dependency injections
	7) Make use of extensions  
	8) Break the functions into classes 