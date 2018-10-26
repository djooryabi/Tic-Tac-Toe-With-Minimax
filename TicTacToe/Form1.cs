using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace TicTacToe
{
    public partial class Form1 : Form
    {
        // Current decision node
        private StateNode currentNode;
        // Buttons used in windows form
        private Button[] buttons = new Button[9];
        // Keep a count of all the game states generated for learning purposes
        private int count;
        private bool gameRunning = true;

        // Class that represents a game state node
        public class StateNode
        {
            public GameState gs;
            public StateNode parent;
            public List<StateNode> children;
            public int minimaxValue;
            public char player;
            public Tuple<int, int> action = new Tuple<int, int>(-1, -1);
            public StateNode()
            {
                gs = new GameState();
                children = new List<StateNode>();
            }
        }

        // Class that represents the tic-tac-toe board at a point in time
        public class GameState
        {
            public char[,] state;

            public GameState()
            {
                state = new char[3, 3];
            }

            /// <summary>
            /// Checks if this game state is equal to another game state
            /// </summary>
            /// <param name="gs"></param>
            /// <returns></returns>
            public bool Equals(GameState gs)
            {

                for (var i = 0; i < state.GetLength(0); i++)
                {
                    for (var j = 0; j < state.GetLength(1); j++)
                    {
                        if (state[i, j] != gs.state[i, j])
                        {
                            return false;
                        }
                    }
                }

                return true;
            }

            /// <summary>
            /// Returns a fresh copy of this game state
            /// </summary>
            /// <returns></returns>
            public GameState Copy()
            {
                var copy = new GameState();
                copy.state = new char[3, 3];

                for (var i = 0; i < 3; i++)
                {
                    for (var j = 0; j < 3; j++)
                    {
                        copy.state[i, j] = state[i, j];
                    }
                }
                return copy;
            }
        }

        public Form1()
        {
            InitializeComponent();
            buttons[0] = button1;
            buttons[1] = button2;
            buttons[2] = button3;
            buttons[3] = button4;
            buttons[4] = button5;
            buttons[5] = button6;
            buttons[6] = button7;
            buttons[7] = button8;
            buttons[8] = button9;

            currentNode = new StateNode();
            currentNode.gs = new GameState();
            currentNode.gs.state = new char[3, 3];
            currentNode.player = 'x';

            // permute over all possible game states. This will take a couple seconds please be patient.
            GenerateStates(currentNode, 'x');
            var terminalNodes = new List<StateNode>();
            GetTerminalNodes(currentNode, terminalNodes);
            Debug.WriteLine("Generated " + terminalNodes.Count + " terminal nodes");
            Debug.WriteLine("Generated " + count + " nodes");
        }

        private bool IsTerminalState(GameState gs)
        {
            return IsWinningState('x', gs) == true || IsWinningState('o', gs) == true || IsDraw(gs) == true;
        }

        /// <summary>
        /// Main AI function that generates all possible tic-tac-toe game outcomes and stores it in ram for the AI to use when playing.
        /// </summary>
        /// <param name="root"></param>
        /// <param name="startingPlayer"></param>
        private void GenerateStates(StateNode root, char startingPlayer)
        {
            // count the number of game states generated
            count++;

            // start when X moves first
            for (var i = 0; i < 3; i++)
            {
                for (var j = 0; j < 3; j++)
                {
                    if (root.gs.state[i, j] == 0 && IsTerminalState(root.gs) == false)
                    {
                        var copiedGameState = root.gs.Copy();
                        copiedGameState.state[i, j] = startingPlayer;

                        var newNode = new StateNode();
                        newNode.gs = copiedGameState;
                        newNode.parent = root;
                        root.children.Add(newNode);

                        newNode.action = Tuple.Create(i, j);

                        if (startingPlayer == 'x')
                        {
                            newNode.player = 'o';
                            GenerateStates(newNode, 'o');
                        }
                        else if (startingPlayer == 'o')
                        {
                            newNode.player = 'x';
                            GenerateStates(newNode, 'x');
                        }
                        else
                        {
                            Console.WriteLine("Error: Unknown button value");
                            return;
                        }
                    }
                }
            }

            // If we reach a terminal node then it's minimax value is simply the outcome of the game since there are no more moves to play
            if (root.children.Count == 0)
            {
                root.minimaxValue = Utility(root);
            }
            else
            {
                if (root.player == 'x')
                {
                    // the max player
                    var max = int.MinValue;

                    foreach (var child in root.children)
                    {
                        if (child.minimaxValue > max)
                        {
                            max = child.minimaxValue;
                        }
                    }

                    root.minimaxValue = max;
                }
                else
                {
                    // min player
                    var min = int.MaxValue;

                    foreach (var child in root.children)
                    {
                        if (child.minimaxValue < min)
                        {
                            min = child.minimaxValue;
                        }
                    }

                    root.minimaxValue = min;
                }
            }

        }

        private void AIMove()
        {

            if (gameRunning == true)
            {

                // choose the min minimax value
                StateNode minNode = null;
                var min = int.MaxValue;

                foreach (var n in currentNode.children)
                {
                    if (n.minimaxValue < min)
                    {
                        min = n.minimaxValue;
                        minNode = n;
                    }
                }

                if (minNode != null)
                {
                    // perform the ai move now
                    FillWithO(IndexToButtonNumber(minNode.action.Item1, minNode.action.Item2));
                    Debug.WriteLine("AI moving to " + minNode.action.Item1 + ", " + minNode.action.Item2);
                }
                else
                {
                    Debug.WriteLine("Error node is null");
                    return;
                }

            }
        }

        /// <summary>
        /// This function returns how good a particular move is for the AI to make. 0 means the move will lead to a tie, 1 means the move will lead to the AI towards victory, -1 means 
        /// the AI will get closer to losing if it choses that move.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private int Utility(StateNode node)
        {
            var result = 0;

            if (IsWinningState('x', node.gs) == true)
            {
                result = 1;
            }
            else if (IsWinningState('o', node.gs) == true)
            {
                result = -1;
            }
            else
            {
                result = 0;
            }

            return result;
        }
        private int IndexToButtonNumber(int row, int col)
        {
            return row * 3 + col + 1;
        }

        /// <summary>
        /// Function that checks if a gamestate is game over because of a player winning or a tie
        /// </summary>
        /// <param name="player"></param>
        /// <param name="gs"></param>
        /// <returns></returns>
        private bool IsWinningState(char player, GameState gs)
        {
            // check horizontally
            for (var i = 0; i < 3; i++)
            {
                var horizontalCount = 0;
                for (var j = 0; j < 3; j++)
                {
                    if (gs.state[i, j] == player)
                    {
                        horizontalCount++;
                    }
                }

                if (horizontalCount == 3)
                {
                    return true;
                }
            }

            // check veritcally
            for (var i = 0; i < 3; i++)
            {
                var verticalCount = 0;

                for (var j = 0; j < 3; j++)
                {
                    if (gs.state[j, i] == player)
                    {
                        verticalCount++;
                    }
                }

                if (verticalCount == 3)
                {
                    return true;
                }
            }

            // check both diagonals

            if (gs.state[0, 0] == player && gs.state[1, 1] == player && gs.state[2, 2] == player)
            {
                return true;
            }
            else if (gs.state[2, 0] == player && gs.state[1, 1] == player && gs.state[0, 2] == player)
            {
                return true;
            }

            return false;
        }

        private bool IsDraw(GameState gs)
        {
            var filledInSpots = 0;

            for (var i = 0; i < 3; i++)
            {
                for (var j = 0; j < 3; j++)
                {
                    if (gs.state[i, j] != 0)
                    {
                        filledInSpots++;
                    }
                }
            }

            if (filledInSpots == 9 && IsWinningState('x', gs) == false && IsWinningState('o', gs) == false)
            {
                return true;
            }

            return false;
        }

        private void GetTerminalNodes(StateNode root, List<StateNode> terminalNodes)
        {
            if (root.children.Count == 0)
            {
                terminalNodes.Add(root);
            }
            else
            {
                foreach (var node in root.children)
                {
                    GetTerminalNodes(node, terminalNodes);
                }
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            FillWithX(1);
            AIMove();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            FillWithX(2);
            AIMove();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            FillWithX(3);
            AIMove();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            FillWithX(4);
            AIMove();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            FillWithX(5);
            AIMove();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            FillWithX(6);
            AIMove();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            FillWithX(7);
            AIMove();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            FillWithX(8);
            AIMove();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            FillWithX(9);
            AIMove();
        }

        private void CheckState()
        {

            if (IsWinningState('o', currentNode.gs) == true)
            {
                textBox1.Text = "Computer won!";
                gameRunning = false;
            }
            else if (IsWinningState('x', currentNode.gs) == true)
            {
                textBox1.Text = "Player won!";
                gameRunning = false;
            }
            else if (IsDraw(currentNode.gs) == true)
            {
                textBox1.Text = "Draw";
                gameRunning = false;
            }
        }

        /// <summary>
        /// Important function that is used by players to make a move in the game and move down the decision tree.
        /// </summary>
        /// <param name="action"></param>
        private void MoveThroughTree(Tuple<int, int> action)
        {
            foreach (var n in currentNode.children)
            {
                if (n.action.Item1 == action.Item1 && n.action.Item2 == action.Item2)
                {
                    currentNode = n;
                    Debug.WriteLine("Moving down tree");
                    return;
                }
            }
        }

        private void FillWithX(int buttonNumber)
        {
            if (gameRunning == true)
            {
                var row = (buttonNumber - 1) / 3;
                var col = (buttonNumber - 1) % 3;

                MoveThroughTree(Tuple.Create(row, col));
                currentNode.gs.state[row, col] = 'x';
                buttons[buttonNumber - 1].BackgroundImage = Image.FromFile(@"../../Image/x.png");
                buttons[buttonNumber - 1].BackgroundImageLayout = ImageLayout.Stretch;
                buttons[buttonNumber - 1].Enabled = false;

                CheckState();
            }
        }

        private void FillWithO(int buttonNumber)
        {
            if (gameRunning == true)
            {
                var row = (buttonNumber - 1) / 3;
                var col = (buttonNumber - 1) % 3;
                MoveThroughTree(Tuple.Create(row, col));
                currentNode.gs.state[row, col] = 'o';
                buttons[buttonNumber - 1].BackgroundImage = Image.FromFile(@"../../Image/o.jpg");
                buttons[buttonNumber - 1].BackgroundImageLayout = ImageLayout.Stretch;
                buttons[buttonNumber - 1].Enabled = false;

                CheckState();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            AllocConsole();
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        public bool ValidIndex(int row, int col)
        {
            return row >= 0 && row < 3 && col >= 0 && col < 3;
        }
    }
}
