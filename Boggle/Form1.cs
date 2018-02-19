using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.IO;


namespace Boggle
{
    public partial class Form1 : Form
    {
        //Creates arrays for textboxes to facilitate receiving and sending values
        TextBox[,] textBoxArray = new TextBox[4, 4];
        //Creates array for values of textboxes to facilitate calculations
        string[,] stringArray = new string [4,4];
        bool debugMode = false;
        string connectionString;
        public Form1()
        {
            InitializeComponent();
            //nested loop to populate textbox array
            for (int row = 0;row<4; row++)
            {
                for (int col = 0; col<4; col++)
                {
                    textBoxArray[row, col] = this.Controls.Find(
                        //name of textbox to add
                        "cellr" + row.ToString() + "c" + col.ToString(), true).FirstOrDefault() as TextBox;

                    Debug.WriteLine("Textbox cellr" + row.ToString() + "c" + col.ToString() + " added to array.");
                }
            }
            //end of nested for loop for textBoxArray initialization
            this.debugModeToolStripMenuItem.CheckOnClick = true;
        }
        Stack<int[]> directionsTemplate()
        {
                Stack<int[]> directions = new Stack<int[]>();
                directions.Push(new int[] { -1, 0 });
                directions.Push(new int[] { -1, 1 });
                directions.Push(new int[] { 0, 1 });
                directions.Push(new int[] { 1, 1 });
                directions.Push(new int[] { 1, 0 });
                directions.Push(new int[] { 1, -1 });
                directions.Push(new int[] { 0, -1 });
                directions.Push(new int[] { -1, -1 });
                return new Stack<int[]>(new Stack<int[]>(directions));
        }

        

        private void btnSolve_Click(object sender, EventArgs e)
        {
            StreamWriter validWords = new StreamWriter(@"..\\validWords.txt");
            //nested loop to initialize stringArray
            for (int row = 0;row<4; row++)
            {
                for (int col = 0; col<4; col++)
                {
                    string inputValue = textBoxArray[row, col].Text;
                    stringArray[row, col] = inputValue;
                    Debug.WriteLineIf(debugMode,
                        "Value " + inputValue + " from textbox cellr" + row.ToString() + "c" + col.ToString() + " added to array.");
                }
            } //end of nested for loop for stringArray initialization
            
            Stack<string>[] wordsByLength = new Stack<string>[14];

            for (int i = 0; i < 14; i++)
                wordsByLength[i] = new Stack<string>();
            
            //nested loop to cycle through possible string combinations
            //outer nested loop cycles through each cell as a starting point
            for (int row = 0; row < 4; row++)
            {
                for (int col = 0; col < 4; col++)
                {
                    /*
                     * path keeps track of what steps we have taken
                     * define new path at this point because we have a new starting point
                     * lastStep is used as reference from which to take the next step, if any
                     * stackOfAdjacentCellsNotChecked keeps a stack of arrays of directions from it's 
                     * corresponding 'index' in the path stack that have not been checked
                     */
                    int[] lastStep = new int[] { row, col };
                    Debug.WriteLineIf(debugMode, "path = " + arrayToString(lastStep));
                    Stack<int[]> path = new Stack<int[]>();
                    path.Push(lastStep);
                    Stack<Stack<int[]>> stackOfAdjacentCellsNotChecked = new Stack<Stack<int[]>>();
                    stackOfAdjacentCellsNotChecked.Push(directionsTemplate());
                    string wordBuild = stringArray[row, col];
                    Debug.WriteLine("wordBuild = " + wordBuild);


                    while (path.Count() > 0)
                    {
                       
                        Stack<int[]> adjacentCellsNotChecked = stackOfAdjacentCellsNotChecked.Pop();
                        //uses findNextStep to return a valid adjacent cell

                        lastStep = findNextStep(lastStep, path, ref adjacentCellsNotChecked, ref stackOfAdjacentCellsNotChecked);

                        //if there is a valid next step, take it and determine if it makes a word
                        //if not, backtrack to the previous step and repeat
                        if (!lastStep.SequenceEqual( new int[] { -1, -1 }))
                        {
                            //redefines the lastStep as the step we just took in order to reference
                            //it for future steps
                            path.Push(lastStep);
                            Debug.WriteLineIf(debugMode, "Pushing step " + arrayToString(lastStep) + "to path. Path height = " + path.Count.ToString());
                            Debug.WriteLineIf(debugMode, "stackOfAdjacentCellsNotChecked height = " + stackOfAdjacentCellsNotChecked.Count);
                            Debug.WriteLineIf(debugMode, "directionsTemplate height = " + stackOfAdjacentCellsNotChecked.Peek().Count.ToString());
                            Debug.WriteLineIf(debugMode, "Pushing new set of directions to stackOfAdjacentCellsNotChecked. stackOfAdjacentCellsNotChecked height = " + stackOfAdjacentCellsNotChecked.Count.ToString());
                            //concatenates the current word string with the next string
                            wordBuild = wordBuild + stringArray[lastStep[0], lastStep[1]];
                            
                            Debug.WriteLine("wordbuild = " + wordBuild);

                            //evaluates if word build is a valid english word or subWord
                            //writes to output file if it's a word
                            //continues evaluating from location if it's a subWord
                            //bactracks if it's not a subWord
                            if (wordBuild.Length>2)
                            {
                                /*
                                string filename = wordBuild.Substring(0, 3) + "_";
                                string filepath = @"..\\SegmentedWordDictionary\\" + filename + ".txt";
                                if (File.Exists(filepath))
                                {
                                    bool word = false;
                                    bool subWord = false;
                                    StreamReader readfile = new StreamReader(filepath);
                                    for (string line = ""; line!=null; line = readfile.ReadLine())
                                    {
                                        if (!word)
                                        {
                                            word = Regex.IsMatch(line, @"^" + wordBuild.ToUpper() + "$");
                                            Debug.WriteLineIf(word, wordBuild + " is a word: " + word);
                                        }
                                        if(!subWord)
                                            subWord = Regex.IsMatch(line, @"^" + wordBuild.ToUpper());
                                    }
                                    readfile.Close();
                                    if (word)
                                    {
                                        wordsByLength[wordBuild.Length - 3].Push(wordBuild);
                                        //validWords.WriteLine(wordBuild);
                                    }
                                    if (!subWord)
                                    {
                                        Debug.WriteLineIf(debugMode, "Word build is not a subword. Bactracking.");
                                        backtrack(ref path, ref stackOfAdjacentCellsNotChecked, ref lastStep, ref wordBuild);
                                    }
                                }
                                else
                                {
                                    //Since there is no corresponding textfile, the current wordbuild will not yield any words
                                    if (stackOfAdjacentCellsNotChecked.Count > 2)
                                    {
                                        Debug.WriteLineIf(debugMode, "Corresponding File not Found. Bactracking.");
                                        backtrack(ref path, ref stackOfAdjacentCellsNotChecked, ref lastStep, ref wordBuild);
                                    }
                                }*/
                            }
                        }
                        else
                        {
                            //all possible paths from the reference point have been exhausted
                            //remove current reference points and return to the previous reference point
                            Debug.WriteLineIf(debugMode, "Dead End. Bactracking.");
                            backtrack(ref path, ref stackOfAdjacentCellsNotChecked, ref lastStep, ref wordBuild);
                        }
                        
                    }//end of while looping through all possible points from the starting point of the path
                }//end loop cycling through cell cols
            }//end loop cycling through cell rows

            //end nested loop to cycle through possible string combinations
            for (int i = 14; i > 0; i--)
            {
                foreach (string word in wordsByLength[i-1])
                { validWords.WriteLine(word); }
            }
                validWords.Close();    

        }//end of code block for btnSolve_Click

        int[] findNextStep( int[] lastStep, Stack<int[]> path, ref Stack<int[]> adjacentCellsNotChecked, ref Stack<Stack<int[]>> stackOfAdjacentCellsNotChecked)
        {
            for (int i = 0; i < adjacentCellsNotChecked.Count();)
            {
                int[] nextStep = adjacentCellsNotChecked.Pop();
                //evaluates if the next step will be within the bounds of the board/grid
                int lowerbound = 0;
                int upperbound = 3;
                if (debugMode)
                    Debug.Write("Attempting to move " + arrayToString(nextStep) + " from " + arrayToString(lastStep));
                nextStep[0] = lastStep[0] + nextStep[0];
                nextStep[1] = lastStep[1] + nextStep[1];
                if ((nextStep[0] >= lowerbound) && (nextStep[0] <= upperbound) && (nextStep[1] >= lowerbound) && (nextStep[1] <= upperbound))
                {
                    //defines final location of nextStep
                    if(debugMode)
                        Debug.Write(" Found possible location " + arrayToString(nextStep)+". ");
                    //checks if that location has already been used, if not take the step
                    if (!path.Any(a => nextStep.SequenceEqual(a)))
                    {
                        stackOfAdjacentCellsNotChecked.Push(adjacentCellsNotChecked);
                        stackOfAdjacentCellsNotChecked.Push(directionsTemplate());
                        Debug.WriteLineIf(debugMode, "Taking step to " + arrayToString(nextStep));
                        return nextStep;
                    }
                    else
                    {
                        Debug.WriteLineIf(debugMode, "Location " + arrayToString(nextStep) + " all ready visited");
                    }
                }
                else
                {
                    Debug.WriteLineIf(debugMode, " Suggested location " + arrayToString(nextStep) + " out of bounds");
                }
            }//returns a next step of {0,0} to indicate no available step was found
            Debug.WriteLineIf(debugMode, "No available locations");
            return new int[] {-1,-1};
        } //end of findNextStep method

        void backtrack(ref Stack<int[]> path, ref Stack<Stack<int[]>> stackOfAdjacentCellsNotChecked, ref int[] lastStep, ref string wordBuild)
        {
            path.Pop();
            while (stackOfAdjacentCellsNotChecked.Count != path.Count)
                stackOfAdjacentCellsNotChecked.Pop();
            if (path.Count != 0)
                lastStep = path.Peek();
            if (debugMode)
                Debug.Write("Reducing word build from " + wordBuild);
            wordBuild = wordBuild.Remove(wordBuild.Length - 1, 1);
            Debug.WriteLineIf(debugMode, " to " + wordBuild);
        }

        string arrayToString(int[] array)
        {
            string arrayString = "{";
            foreach (int val in array)
            {
                arrayString = arrayString + val.ToString() + ", ";
            }
            arrayString = arrayString.Remove(arrayString.Length-2, 2) + "}";
            return arrayString;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            int[] i = { 4, 5 };
        }

        private void debugModeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            debugMode = !debugMode;
        }

        private void button1_Click_1(object sender, EventArgs e)
        {

            StreamReader wordList = new StreamReader(@"..\\wordList.txt");
            
            for (string line = ""; line != null; line = wordList.ReadLine())
            {
                SqlConnection connection = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Felix_Dragonhammer\Desktop\BoggleDataBase\Boggle\dbWordList.mdf;Integrated Security=True");
                SqlCommand command = new SqlCommand("addWord", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@word", line);
                connection.Open();
                int i = command.ExecuteNonQuery();
                connection.Close();
            }
                

            
           

        }
    }
}
