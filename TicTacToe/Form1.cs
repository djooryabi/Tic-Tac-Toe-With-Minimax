using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace TicTacToe
{
    public partial class Form1 : Form
    {
        private GameState currentState;
        private StateNode currentNode;
        private Button[] buttons = new Button[9];
        private List<GameState> terminalStates = new List<GameState>();
        private StateNode rootNode;
        private int count;
        private List<GameState> countedStates = new List<GameState>();
        private readonly char ai = 'o';
        private readonly char maxPlayer = 'x';
        private bool gameRunning = true;

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

        public bool AlreadyCountedThisState(GameState gs)
        {
            foreach (var g in countedStates)
            {
                if (g.Equals(gs) == true)
                {
                    return true;
                }
            }

            return false;
        }

        public class GameState
        {
            public char[,] state;

            public GameState()
            {
                state = new char[3, 3];
            }

            public bool Equals(GameState gs)
            {

                for (var i = 0; i < state.GetLength(0); i++)
                {
                    for (var j = 0; j < state.GetLength(1); j++)
                    {
                        if (state[i,j] != gs.state[i,j])
                        {
                            return false;
                        }
                    }
                }

                return true;
            }

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

            currentState = new GameState();
            currentState.state = new char[3, 3];

            rootNode = new StateNode();
            rootNode.player = 'x';
            Console.WriteLine("Program starting");
            // permute over all possible game states. This will take some memory!
            GenerateStates(rootNode, 'x');
            var terminalNodes = new List<StateNode>();
            GetTerminalNodes(rootNode, terminalNodes);
            Debug.WriteLine("Generated " + terminalNodes.Count + " terminal nodes");
            Debug.WriteLine("Generated " + count + " nodes");
            currentNode = rootNode;
        }

        private bool Contains(List<GameState> gameStates, GameState gs)
        {
            foreach (var g in gameStates)
            {
                if (g.Equals(gs) == true)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsTerminalState(GameState gs)
        {
            return IsWinningState('x', gs) == true || IsWinningState('o', gs) == true || IsDraw(gs) == true;
        }

        private void GenerateStates(StateNode root, char startingPlayer)
        {
            count++;

            // start when X moves first
            for (var i = 0; i < 3; i++)
            {
                for (var j = 0; j < 3; j++)
                {
                    if (root.gs.state[i,j] == 0 && IsTerminalState(root.gs) == false)
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
                        } else if (startingPlayer == 'o')
                        {
                            newNode.player = 'x';
                            GenerateStates(newNode, 'x');
                        } else
                        {
                            Console.WriteLine("Error: Unknown button value");
                            return;
                        }
                    }
                }
            }

            // find the minimax value of this node
            if (root.children.Count == 0)
            {
                root.minimaxValue = Utility(root);

                if (root.minimaxValue != 0)
                {
                    //Debug.WriteLine("Minimax value is " + root.minimaxValue);
                }
            } else
            {
                if (root.player == maxPlayer)
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
                if (ai == 'x')
                {
                    // choose the max minimax value
                    StateNode maxNode = null;

                    var max = int.MinValue;

                    foreach (var n in currentNode.children)
                    {
                        if (n.minimaxValue > max)
                        {
                            max = n.minimaxValue;
                            maxNode = n;
                        }
                    }

                    if (maxNode != null)
                    {
                        // perform the ai move now
                        FillWithX(IndexToButtonNumber(maxNode.action.Item1, maxNode.action.Item2));
                        Debug.WriteLine("AI moving to " + maxNode.action.Item1 + ", " + maxNode.action.Item2);
                    }
                    else
                    {
                        Debug.WriteLine("Error node is null");
                        return;
                    }

                }
                else if (ai == 'o')
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
        }

        private int Utility(StateNode node)
        {
            var result = 0;

            if (IsWinningState('x', node.gs) == true)
            {
                result = 1;
            } else if (IsWinningState('o', node.gs) == true)
            {
                result = -1;
            } else
            {
                result = 0;
            }

            return result;
        }
        private int IndexToButtonNumber(int row, int col)
        {
            return row * 3 + col + 1;
        }

        private bool IsWinningState(char player, GameState gs)
        {
            // check horizontally
            for (var i = 0; i < 3; i++)
            {
                var horizontalCount = 0;
                for (var j = 0; j < 3; j++)
                {
                    if (gs.state[i,j] == player)
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
                    if (gs.state[j,i] == player)
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

            if (gs.state[0,0] == player && gs.state[1,1] == player && gs.state[2,2] == player)
            {
                return true;
            } else if (gs.state[2,0] == player && gs.state[1,1] == player && gs.state[0,2] == player)
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
                    if (gs.state[i,j] != 0)
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
            } else
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
            char player = (ai == 'x') ? 'o' : 'x';

            if (IsWinningState(ai, currentState) == true)
            {
                textBox1.Text = "Computer won!";
                gameRunning = false;
            } else if (IsWinningState(player, currentState) == true)
            {
                textBox1.Text = "Player won!";
                gameRunning = false;
            } else if (IsDraw(currentState) == true)
            {
                textBox1.Text = "Draw";
                gameRunning = false;
            }
        }

        private void MoveThroughTree(Tuple<int,int> action)
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
                currentState.state[row, col] = 'x';
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
                currentState.state[row, col] = 'o';
                buttons[buttonNumber - 1].BackgroundImage = Image.FromFile(@"../../Image/o.jpg");
                buttons[buttonNumber - 1].BackgroundImageLayout = ImageLayout.Stretch;
                buttons[buttonNumber - 1].Enabled = false;

                CheckState();
            }
        }

        private bool IsWinningSpot(int buttonNumber, char player, GameState gs)
        {
            var row = (buttonNumber - 1) / 3;
            var col = (buttonNumber - 1) % 3;

            var horizontalCount = 0;
            // check left horizontally
            for (var i = col; i >= 0; i--)
            {
                if (gs.state[row,i] == player)
                {
                    horizontalCount++;
                }
            }

            // check right horizontally
            for (var i = col+1; i <= 2; i++)
            {
                if (gs.state[row, i] == player)
                {
                    horizontalCount++;
                }
            }

            if (horizontalCount == 3)
            {
                return true;
            }

            var verticalCount = 0;

            // check down vertically
            for (var i = row; i <= 2; i++)
            {
                if (gs.state[i,col] == player)
                {
                    verticalCount++;
                }
            }

            // check up vertically
            for (var i = row-1; i >= 0; i--)
            {
                if (gs.state[i,col] == player)
                {
                    verticalCount++;
                }
            }

            if (verticalCount == 3)
            {
                return true;
            }

            var diagonalCount = 0;

            // check diagonal right down
            for (var i = 0; i < 3; i++)
            {
                if (ValidIndex(row+i, col+i) && gs.state[row+i, col+i] == player)
                {
                    diagonalCount++;
                }
            }

            // check diagonal left up
            for (var i = 0; i < 3; i++)
            {
                if (ValidIndex(row-1-i, col-1-i) && gs.state[row-1-i, col-1-i] == player)
                {
                    diagonalCount++;
                }
            }


            if (diagonalCount == 3)
            {
                return true;
            }

            diagonalCount = 0;
            // check diagonal left down
            for (var i = 0; i < 3; i++)
            {
                if (ValidIndex(row + i, col - i) && gs.state[row + i, col - i] == player)
                {
                    diagonalCount++;
                }
            }

            // check diagonal right up
            for (var i = 0; i < 3; i++)
            {
                if (ValidIndex(row - 1 - i, col - 1 + i) && gs.state[row - 1 - i, col - 1 + i] == player)
                {
                    diagonalCount++;
                }
            }

            if (diagonalCount == 3)
            {
                return true;
            }

            return false;
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
