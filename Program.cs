using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSE5559_Final_Generator
{
	public struct Node
	{
		public Node(Tuple<int, int> cord, string col, bool se)
		{
			Coords = cord;
			Color = col;
			Start = se;
		}

		public Tuple<int, int> Coords { get; set; }
		public string Color { get; set;}
		public bool Start { get; set; }
	}

	class Program
	{
		// values / options to choose for a 6x5 grid (30 tiles) ( can change ) 
		/*
		 * # of paths in addition to main
		 *		> main should probably be ~ 1/3 of total tiles
		 *			- increasing length of main reduces complexity of other paths and the length they can be so less decisions / T junctions
		 *		> path min length and max length
		 *		> if is key path or not
		 *		> alt paths (non-main paths) can create shortcuts (go from main path 3 to main path 12)
		 * If have shop spawn
		 * # of keys
		 * Spawn items (enemies, treasure, etc...)
		 *		> which path and how many paths to spawn on
		 *			- can use paths to create rooms and then spawn monsters just in that room 
		 *			- so could say min number of monsters in treasure room is 1-4 or something
		 *		> minimum amount and maximum amount
		 *		> spawn method
		 *			- curve difficulty (so exponential)
		 *			- linear difficulty
		 *			- random range
		 *		> min spawn difficulty
		 *			- seems to be saying what does the spawn difficulty have to be in order to be spawned
		 *			- probably best to keep at zero, seems unweildy 
		 *		> spawn probability
		 *		> placement method
		 *			- near edges
		 *			- random tile on path
		 */
		Dictionary<int, int> seedsAndScores = new Dictionary<int, int>();	// seed is key, score is value so = (seed, score
		static void Main(string[] args)
		{
			string path = @"C:\Users\Grant\Desktop\GraphInfo";
			string pathWrite = @"C:\Users\Grant\Desktop\Test.txt";
			string[] fileNames = Directory.GetFiles(path);
			Program pf = new Program();
			foreach(string file in fileNames)
			{
				pf.ProcessFile(file);
			}

			string[] seedList = pf.ListOfSeeds();
			System.IO.File.WriteAllLines(pathWrite, seedList);
		}

		string[] ListOfSeeds()
		{
			string[] list = new string[10];
			for (int i = 0; i < 10; i++)
			{
				int seed = seedsAndScores.First().Key;
				int score = seedsAndScores[seed];
				list[i] = "Seed: " + seed.ToString() + ", Score: " + score.ToString();
				seedsAndScores.Remove(seed);
			}
			return list;
		}

		void ProcessFile(string path)
		{
			int seed;   // file seed #
			int scoreOption = 1;    // see GetScore method for explanation
			int sortOption = 1;     // sort scores highest to lowest for 1, and lowest to highest for 2
			int maxNumOfScores = 10;    // max number of scores to have in list
			int score;
			Node[] nodes = new Node[9];

			seed = GetSeed(path);
			string[] lines = System.IO.File.ReadAllLines(path);
			for (int i = 0; i < 9; i++)
			{
				int lineNum = i * 3;
				nodes[i].Coords = GetCoords(lines[lineNum]);
				nodes[i].Color = GetColor(lines[lineNum + 1]);
				nodes[i].Start = isStart(lines[lineNum + 2]);
			}

			// evaluate this files score and update dictionary
			score = GetScore(nodes, scoreOption);
			if (seedsAndScores.Keys.Count == maxNumOfScores)
			{
				int highestSeed = seedsAndScores.First().Key;
				int lowestSeed = highestSeed;
				int highestScore = seedsAndScores[highestSeed];
				int lowestScore = highestScore;
				if (sortOption == 1)
				{
					// For this option want to keep high scores and get rid of low ones
					foreach(KeyValuePair<int, int> pair in seedsAndScores)
					{
						if (pair.Value < lowestScore)
						{
							lowestSeed = pair.Key;
						}
					}

					if (seedsAndScores[lowestSeed] < score)
					{
						// in this case the lowest score in the dictionary is lower than our current score
						seedsAndScores.Remove(lowestSeed);
						seedsAndScores.Add(seed, score);
					}
				}
				else
				{
					// For this option want to keep low scores and get rid of high ones
					foreach(KeyValuePair<int, int> pair in seedsAndScores)
					{
						if (pair.Value > highestScore)
						{
							highestSeed = pair.Key;
						}
					}

					if (seedsAndScores[highestScore] > score)
					{
						// in this case the highest score in the dictionary is higher than our current score
						seedsAndScores.Remove(highestSeed);
						seedsAndScores.Add(seed, score);
					}
				}
			}
			else
			{
				seedsAndScores.Add(seed, score);
			}



		}

		/*	Options denote how score is calculated:
		 *		>1: sum of all paths start and end distance
		 *		>2: only main path start and end
		 *		>3: only alt path start and end
		 * 
		 */
		int GetScore(Node[] nodeList, int option)
		{
			int score = 0;
			if (option == 1)
			{
				score += GetPathDisplacement(nodeList, "green");
				score += GetPathDisplacement(nodeList, "yellow");
				score += GetPathDisplacement(nodeList, "orange");
				score += GetPathDisplacement(nodeList, "blue");
				score += GetPathDisplacement(nodeList, "red");
			}
			else if (option == 2)
			{
				score += GetPathDisplacement(nodeList, "green");
			}
			else
			{
				score += GetPathDisplacement(nodeList, "yellow");
				score += GetPathDisplacement(nodeList, "orange");
				score += GetPathDisplacement(nodeList, "blue");
				score += GetPathDisplacement(nodeList, "red");
			}
			return score;
		}

		int GetPathDisplacement(Node[] nodeList, string givenColor)
		{
			Node startNode = nodeList[0];	//assignment because have to for distance calculation
			Node endNode = nodeList[0];
			for (int i = 0; i < nodeList.Length; i++)
			{
				if(nodeList[i].Color.Equals(givenColor))
				{
					if(nodeList[i].Start)
					{
						startNode = nodeList[i];
					}
					else
					{
						endNode = nodeList[i];
					}
				}
			}

			int val1 = endNode.Coords.Item1 - startNode.Coords.Item1;
			int val2 = endNode.Coords.Item2 - startNode.Coords.Item2;
			return (int)Math.Sqrt((double)((val1 * val1) + (val2 * val2)));
		}

		Tuple<int, int> GetCoords(string line)
		{
			int x, y;
			line = line.Substring(13);
			x = int.Parse(line[0].ToString());
			line = line.Substring(line.Length - 1);
			y = int.Parse(line[0].ToString());
			return new Tuple<int, int>(x, y);
		}

		string GetColor(string line)
		{
			string red, green, color;
			
			//trim string down to just numbers
			line = line.Substring(line.IndexOf('(') + 1);
			line = line.Substring(0, line.Length - 1);

			// put numbers into strings for comparisons
			red = line.Substring(0, 5);
			line = line.Substring(6);
			green = line.Substring(0, 5);

			if (red[0].Equals('1'))
			{
				if (green[2].Equals('9'))
				{
					color = "yellow";
				}
				else if (green[2].Equals('5'))
				{
					color = "orange";
				}
				else
				{
					color = "red";
				}
			}
			else if (red[2].Equals('3'))
			{
				color = "blue";
			}
			else
			{
				color = "green";
			}

			return color;
		}

		bool isStart(string line)
		{
			bool result = false;
			line = line.Substring(line.IndexOf(':') + 2);
			if (line[0].Equals('S'))
			{
				result = true;
			}
			return result;
		}

		int GetSeed(string path)
		{
			int seed;
			bool trimmed = false;
			path = path.Substring(0, path.Length - 4);
			path = path.Substring(path.Length - 6);
			while (!trimmed)
			{
				if (isNum(path[0]))
				{
					trimmed = true;
				}
				else
				{
					path = path.Substring(1);
				}
			}
			seed = int.Parse(path);
			return seed;
		}

		bool isNum(char c)
		{
			bool num = false;
			switch (c)
			{
				case '0':
					num = true;
					break;
				case '1':
					num = true;
					break;
				case '2':
					num = true;
					break;
				case '3':
					num = true;
					break;
				case '4':
					num = true;
					break;
				case '5':
					num = true;
					break;
				case '6':
					num = true;
					break;
				case '7':
					num = true;
					break;
				case '8':
					num = true;
					break;
				case '9':
					num = true;
					break;
				default:
					break;
			}
			return num;
		}
	}
}
