using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Web;

namespace my_graph_crawler {
	//=====================================================================
    // This block defines a class with global variables
    // This solution is thought to be temporary ;)
    //=====================================================================
    internal static class GlobalVariables
    {
        public const char Separator = ';';
        public const string LogPath = @"log.log";
    }



    //=====================================================================
    // This block defines main class.
    // Entry point of the programm is here
    //=====================================================================
	public class main_class {
		public static string HandleFile(string file_name) {
			return Path.GetFileNameWithoutExtension(file_name);
		}

		public static void Main(string[] args) {
			BasicUtilities.LogStart();

			var newGraph = new GraphEntity();
			newGraph.ScanCSV(args.Count() == 0 ? @"generated.csv" : args[0]);
			Console.WriteLine("Scanned successfully!");
			Console.WriteLine("Persons: {0}", newGraph.GetPersonsCount());
			Console.WriteLine("Companies: {0}", newGraph.GetCompaniesCount());

			newGraph.InitGroupIDs();

			newGraph.WriteGraph("persons.csv", "companies.csv");
			
			Console.ReadLine();

			BasicUtilities.LogEnd();
		}
	// End of main_class class
	}



	//=====================================================================
    // This block defines a graph related classes and utilities
    // v.0.1 solution is thought to operate without abstract classes and
    // explicit interface declaration
    // 18.12.11 12:41 - probably fail. will try to use abstractions
    // 18.12.11 15:03 - i'm starting using cludges
    //=====================================================================
	public interface iVertex {
		IEnumerable<string> GetNeighbourIDs(); // !!! it should return IEnumerable<iVertex>
	}

	public abstract class absPerson : iVertex {
		protected string ID;
    	protected HashSet<string> Companies; // !!! it should be HashSet<iVertex>

    	public IEnumerable<string> GetNeighbourIDs() {
    		return Companies;
    	}
	}

	public abstract class absCompany : iVertex {
		protected string ID;
    	protected HashSet<string> Persons; // !!! it should be HashSet<iVertex>
    	protected HashSet<string> Companies; // !!! it should be HashSet<iVertex>

    	// !!! broken logic here
    	public IEnumerable<string> GetNeighbourIDs() {
    		return Persons;
    	}
	}



	public class PersonEntity : absPerson {
    	protected int GroupID;

    	public PersonEntity() {
    		ID = "";
    		Companies = new HashSet<string>();
    		GroupID = 0;
    	}

    	public PersonEntity(string inID, IEnumerable<string> inCompanies, int inGroupID=0) {
    		ID = inID;
    		Companies = new HashSet<string>(inCompanies);
    		GroupID = inGroupID;
    	}

    	public PersonEntity(string inID, string inCompany=null, int inGroupID=0) {
    		ID = inID;
    		Companies = new HashSet<string>();
    		if (inCompany != null) Companies.Add(inCompany);
    		GroupID = inGroupID;
    	}

    	// Getters and setters
    	public string GetID() {
    		return ID;
    	}

    	public int GetGroupID() {
    		return GroupID;
    	}

    	public HashSet<string> GetCompanies() {
    		return Companies;
    	}

    	public void SetGroupID(int inGroupID) {
    		GroupID = inGroupID;
    	}

    	// Augmentation
    	public bool AddCompany(string inCompany) {
    		return Companies.Add(inCompany);
    	}
    }



    public class CompanyEntity : absCompany {
    	protected int GroupID;
    	
    	public CompanyEntity() {
    		ID = "";
    		Persons = new HashSet<string>();
    		Companies = new HashSet<string>();
    		GroupID = 0;
    	}

    	public CompanyEntity(string inID, IEnumerable<string> inPersons, IEnumerable<string> inCompanies, int inGroupID=0) {
    		ID = inID;
    		Persons = new HashSet<string>(inPersons);
    		Companies = new HashSet<string>(inCompanies);
    		GroupID = inGroupID;
    	}

    	public CompanyEntity(string inID, string inPerson=null, string inCompany=null, int inGroupID=0) {
    		ID = inID;
    		Persons = new HashSet<string>();
    		if (inPerson != null) Persons.Add(inPerson);
    		Companies = new HashSet<string>();
    		if (inCompany != null) Companies.Add(inCompany);
    		GroupID = inGroupID;
    	}

    	// Getters and setters
    	public string GetID() {
    		return ID;
    	}

    	public int GetGroupID() {
    		return GroupID;
    	}

    	public HashSet<string> GetCompanies() {
    		return Companies;
    	}

    	public void SetGroupID(int inGroupID) {
    		GroupID = inGroupID;
    	}

    	// Augmentation
    	public bool AddPerson(string inPerson) {
    		return Persons.Add(inPerson);
    	}

    	public bool AddCompany(string inCompany) {
    		return Companies.Add(inCompany);
    	}
    }



    public class GraphEntity {
    	protected SortedDictionary<string, PersonEntity> Persons;
    	protected SortedDictionary<string, CompanyEntity> Companies;



    	public GraphEntity() {
    		Persons = new SortedDictionary<string, PersonEntity> ();
    		Companies = new SortedDictionary<string, CompanyEntity> ();
    	}



    	public int GetPersonsCount() {
    		return Persons.Keys.Count();
    	}

    	public int GetCompaniesCount() {
    		return Companies.Keys.Count();
    	}

    	public PersonEntity GetPersonByID(string inID) {
    		var proxy = new PersonEntity (inID);
    		if (Persons.TryGetValue(inID, out proxy)) return proxy;
    		else throw new System.ArgumentException("Can't find requested element", "original");
    	}

		public CompanyEntity GetCompanyByID(string inID) {
    		var proxy = new CompanyEntity (inID);
    		if (Companies.TryGetValue(inID, out proxy)) return proxy;
    		else throw new System.ArgumentException("Can't find requested element", "original");
    	}

    	public void AddNewPCEdge(string inPersonID, string inCompanyID) {
    		var proxyPerson = new PersonEntity (inPersonID);
    		var proxyCompany = new CompanyEntity (inCompanyID);

    		if (Persons.ContainsKey(inPersonID)) Persons.TryGetValue(inPersonID, out proxyPerson);
    		else Persons.Add(inPersonID, proxyPerson);

    		if (Companies.ContainsKey(inCompanyID)) Companies.TryGetValue(inCompanyID, out proxyCompany);
    		else Companies.Add(inCompanyID, proxyCompany);

    		proxyPerson.AddCompany(inCompanyID);
    		proxyCompany.AddPerson(inPersonID);
    	}

    	public void InitGroupIDs() {
    		int index = 1;
    		int iterationCounter = 0;

    		var stackPersonIDs = new Stack<string>();
    		var personsToCheck = new HashSet<string>(Persons.Keys);
    		var visitedPersons = new HashSet<string>();

    		var stackCompanyIDs = new Stack<string>();
    		var companiesToCheck = new HashSet<string>(Companies.Keys);
    		var visitedCompanies = new HashSet<string>();

    		//stackPersonIDs.Push(personsToCheck.First());
    		var tmpID = personsToCheck.First();
    		stackPersonIDs.Push(tmpID);
    		personsToCheck.Remove(tmpID);

    		while (personsToCheck.Count() + companiesToCheck.Count() > 0) {
    			while (stackPersonIDs.Count() + stackCompanyIDs.Count() > 0) {
    				if (stackPersonIDs.Count() > 0) {
    					string IDToCheck = stackPersonIDs.Pop();
    					var vertexToCheck = this.GetPersonByID(IDToCheck);
    					vertexToCheck.SetGroupID(index);
    					foreach (var el in vertexToCheck.GetNeighbourIDs()) {
    						if (companiesToCheck.Contains(el)) {
    							stackCompanyIDs.Push(el);
    							companiesToCheck.Remove(el);
    						}
    					}
    					visitedPersons.Add(IDToCheck); // could be removed harmlessly
    				}
    				else {
    					string IDToCheck = stackCompanyIDs.Pop();
    					var vertexToCheck = this.GetCompanyByID(IDToCheck);
    					vertexToCheck.SetGroupID(index);
    					foreach (var el in vertexToCheck.GetNeighbourIDs()) {
    						if (personsToCheck.Contains(el)) {
    							stackPersonIDs.Push(el);
    							personsToCheck.Remove(el);
    						}
    					}
    					visitedCompanies.Add(IDToCheck); // could be removed harmlessly
    				}
    		    	//string toCheck = personToCheck ? stackPersonIDs.Pop() : stackCompanyIDs.Pop();
    		    	iterationCounter += 1;
    		    	if (iterationCounter % 1000 == 0) Console.WriteLine(DateTime.Now + ": {0} iterations done!", iterationCounter);
    		    }

    		    if (personsToCheck.Count() > 0) {
    		    	tmpID = personsToCheck.First();
    		    	stackPersonIDs.Push(personsToCheck.First());
    		    	personsToCheck.Remove(tmpID);
    		    }
    			else if (companiesToCheck.Count() > 0) {
    				tmpID = companiesToCheck.First();
    				stackCompanyIDs.Push(companiesToCheck.First()); // !!! should not work, fix later for exception throwing
    				companiesToCheck.Remove(tmpID);
    			}
    			else break;
    			index += 1;
    		}
    	}

    	public void ScanCSV(string inFilePath) {
    		using (var inputStream = new StreamReader(inFilePath)) {
    			while (!inputStream.EndOfStream) {
    				var readString = inputStream.ReadLine().Split(GlobalVariables.Separator);
    				this.AddNewPCEdge(readString[1], readString[0]);
    			}
    		}
    	}

    	public void WriteGraph(string inPersonsFilePath, string inCompaniesFilePath) {
    		using (var outputStream = new StreamWriter(inPersonsFilePath)) {
    			foreach (var i in this.Persons.Values) {
    				outputStream.WriteLine("{0};[{1}];{2}", i.GetID(), string.Join(",", i.GetNeighbourIDs()), i.GetGroupID());
    			}
    		}

    		using (var outputStream = new StreamWriter(inCompaniesFilePath)) {
    			foreach (var i in this.Companies.Values) {
    				outputStream.WriteLine("{0};[{1}];{2}", i.GetID(), string.Join(",", i.GetNeighbourIDs()), i.GetGroupID());
    			}
    		}
    	}
    }

	//=====================================================================
    // This block defines basic utilities such as logger.
    // Everything not specific should be here
    //=====================================================================
	public class BasicUtilities
    {
        public static void LogStart()
        {
            using (var log = new StreamWriter(GlobalVariables.LogPath, true))
            {
                log.WriteLine(DateTime.Now + " - graph_crawler launched!");
            }
        }
        
        public static void LogEnd()
        {
            using (var log = new StreamWriter(GlobalVariables.LogPath, true))
            {
                log.WriteLine(DateTime.Now + " - graph_crawler reached its endpoint successfully!");
            }
        }

        public static void UniversalLogger(string message = "No message specified!", string path = "log.log")
        {
            using (var log = new StreamWriter(path, true))
            {
                log.WriteLine(DateTime.Now + " - " + message);
            }
        }
    }



// End of my_graph_crawler namespace
}