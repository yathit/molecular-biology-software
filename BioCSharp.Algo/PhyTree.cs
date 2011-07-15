using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows;
using System.ComponentModel;

namespace BioCSharp.Algo.Phylo
{
    /// <summary>
    /// Phylogenetic tree object
    /// </summary>
    public class Tree
    {
        #region members
        private string name;

        private List<Leaf> leaves;
        private List<Branch> branches;
        /// <summary>
        /// Flag for valication requirement
        /// </summary>
        private bool invalid = true;

        #endregion

        #region constructors
        /// <summary>
        /// Create phylogenetic tree
        /// </summary>
        /// <param name="B">B is a numeric array of size [<code>NumOfBranches</code> X 2]
        /// in which every row represents
        /// a branch of the tree and it contains two pointers to the branches 
        /// or leaves nodes which are its children.</param>
        /// <param name="names">Name of leave</param>
        /// <param name="dist">a numeric array of size <code>NumOfNodes</code> with 
        /// the distances of every child node (leaf or branch) to its parent
        /// branch.</param>
        public Tree(int[,] B, string[] names, double[] dist)
        {
            this.Name = "";

            CreateTree(B, names, dist);
        }

        /// <summary>
        /// creates an ultrametric phylogenetic tree object 
        /// </summary>
        /// <param name="B">B is a numeric array of size [<code>NumOfBranches</code> X 2]
        /// in which every row represents
        /// a branch of the tree and it contains two pointers to the branches 
        /// or leaves nodes which are its children. Leaf nodes are numbered from 1
        /// to <code>NumOfLeaves</code> and branch nodes are numbered from <code>NumOfLeaves</code> + 1 to
        /// <code>NumOfLeaves</code> + <code>NumOfBranches</code>. Note that since only binary trees are allowed,
        /// then <code>NumOfLeaves</code> = <code>NumOfBranches</code> + 1.</param>
        public Tree(int[,] B)
        {
            this.Name = "";

            CreateTree(B);
        }

        /// <summary>
        /// Create phylogenetic tree from a NEWICK tree formatted file.
        /// 
        /// The NEWICK tree format is found at: 
        /// http://evolution.genetics.washington.edu/phylip/newicktree.html
        /// </summary>
        /// <param name="fileName"></param>
        public Tree(string strin)
        {
            this.Name = "";
            // StreamReader reader = File.OpenText(fileName);
            // string strin = reader.ReadToEnd();
            // reader.Close();
            strin = strin.Replace("\r\n", "");

            #region read number of leaves and branches
            int NumOfBranches = 0;
            foreach (char c in strin)
            {
                if (c == ',')
                    ++NumOfBranches;
            }

            int NumOfLeaves = NumOfBranches + 1;
            int NumOfNodes = NumOfLeaves + NumOfBranches;
            if (NumOfBranches == 0)
            {
                throw new InvalidOperationException("There is not any comma in the data,\ninput string may not be in Newick style or is not a valid filename.");
            }
            //leaves = new List<Leaf>(NumOfLeaves);
            //branches = new List<Branch>(NumOfBranches);
            #endregion


            #region some consistency checking on the parenthesis
            int unpairedParenthesis = 0;
            foreach (char c in strin)
            {
                if (c == '(')
                    ++unpairedParenthesis;
                else if (c == ')')
                    --unpairedParenthesis;

                if (unpairedParenthesis < 0)
                    throw new InvalidOperationException("The parentheses structure is inconsistent,\ninput string may not be in Newick style or is not a valid string.");
            }
            if (unpairedParenthesis != 0)
                throw new InvalidOperationException("The parentheses structure is inconsistent,\ninput string may not be in Newick style or is not a valid string.");
            #endregion

            string[] names = new string[NumOfNodes];
            double[] dist = new double[NumOfNodes];
            int[,] tree = new int[NumOfBranches, 2];


            Regex regLeafData = new Regex(@"[(,][^\(,\);\[\]]+");
            MatchCollection matchesLeafData = regLeafData.Matches(strin);
            Regex regColon = new Regex(@":");
            for (int i = 0; i < matchesLeafData.Count; i++)
            {
                Match m = matchesLeafData[i];
                MatchCollection matchesColon = regColon.Matches(m.Value);
                if (matchesColon.Count == 0)
                    continue;
                int idx = matchesColon[matchesColon.Count - 1].Index;
                names[i] = m.Value.Substring(1, idx - 1);
                if (matchesColon.Count > 0)
                {
                    // if there is colon, get a name 
                    dist[i] = Double.Parse(m.Value.Substring(idx + 1));
                }
            }

            Regex regParenthesis = new Regex(@"\)[^\(,\);\[\]]*");
            MatchCollection matchesParenthesis = regParenthesis.Matches(strin);
            //Regex regColon = new Regex(@":");
            string[] parenthesisData = new string[matchesParenthesis.Count];
            double[] parenthesisDist = new double[matchesParenthesis.Count];
            for (int i = 0; i < matchesParenthesis.Count; i++)
            {
                Match m = matchesParenthesis[i];
                MatchCollection matchesColon = regColon.Matches(m.Value);
                if (matchesColon.Count == 0)
                    continue;
                int idx = matchesColon[matchesColon.Count - 1].Index;
                parenthesisData[i] = m.Value.Substring(1, idx - 1);
                if (matchesColon.Count > 0)
                {
                    // if there is colon, get a name 
                    parenthesisDist[i] = Double.Parse(m.Value.Substring(idx + 1));
                }
            }

            // find the string features: open and close parentheses and leaves
            Regex regLeavePos = new Regex(@"[(,][^(,)]");
            Regex regParenthesisPositions = new Regex(@"[()]");
            List<int> strFeaturesBox = new List<int>();
            MatchCollection mc = regLeavePos.Matches(strin);
            foreach (Match m in mc)
            {
                strFeaturesBox.Add(m.Index + 1);
            }
            mc = regParenthesisPositions.Matches(strin);
            foreach (Match m in mc)
            {
                strFeaturesBox.Add(m.Index);
            }
            strFeaturesBox.Sort();
            string strFeatures = "";
            foreach (int i in strFeaturesBox)
            {
                strFeatures += strin[i];
            }


            int li = 1; int bi = 1; int pi = 1;       // indexes for leaf, branch and parentheses
            int[] queue = new int[2 * NumOfLeaves];   // setting the queue (worst case size)
            int qp = 0;

            // extract label information for the leaves
            int j = 1;
            int bp = 0;
            while (j <= strFeatures.Length)
            {
                switch (strFeatures[j - 1])
                {
                    case ')':
                        // close parenthesis, pull values from the queue to create
                        // a new branch and push the new branch # into the queue
                        int lastOpenPar = 0;
                        for (int k = qp; k >= 1; --k)
                        {
                            if (queue[k - 1] == 0)
                            {
                                lastOpenPar = k;
                                break;
                            }
                        }
                        int numElemInPar = Math.Min(3, qp - lastOpenPar);
                        switch (numElemInPar)
                        {
                            case 2:
                                // 99% of the cases, two elements in the parenthesis
                                bp = bi + NumOfLeaves;
                                names[bp - 1] = parenthesisData[pi - 1];      // set name
                                dist[bp - 1] = parenthesisDist[pi - 1];       // set length
                                tree[bi - 1, 0] = queue[qp - 2];
                                tree[bi - 1, 1] = queue[qp - 1];
                                qp = qp - 2;
                                queue[qp - 1] = bp;
                                bi++;
                                pi++;
                                break;
                            case 3:
                                // find in non-binary trees, create a phantom branch
                                bp = bi + NumOfLeaves;
                                names[bp - 1] = "";      // set name
                                dist[bp - 1] = 0.0;        // set length 
                                tree[bi - 1, 0] = queue[qp - 2];
                                tree[bi - 1, 1] = queue[qp - 1];
                                qp = qp - 1; // writes over the left element
                                queue[qp - 1] = bp;
                                bi = bi + 1;
                                j = j - 1;  // repeat this closing branch to get the rest
                                break;
                            case 1:
                                // parenthesis with no meaning (holds one element)
                                qp = qp - 1;
                                queue[qp - 1] = queue[qp + 1 - 1];
                                pi = pi + 1;
                                break;
                            case 0:
                                throw new InvalidOperationException("Found parenthesis pair with no data,\n" +
                                    "input string may not be in Newick style or" +
                                    "is not a valid filename.");
                        }

                        break;
                    case '(':
                        // an open parenthesis marker (0) pushed into the queue
                        qp = qp + 1;
                        queue[qp - 1] = 0;
                        break;
                    default:
                        // a new leaf pushed into the queue
                        qp = qp + 1;
                        queue[qp - 1] = li;
                        li = li + 1;
                        break;
                } // switch strFeatures
                j = j + 1;
            } // while j ...

            // make 0 base indexing
            for (int i = 0; i < tree.GetLength(0); ++i)
                for (j = 0; j < tree.GetLength(1); ++j)
                    tree[i, j]--;

            CreateTree(tree, names, dist);
        }

        #endregion

        #region Tree creation
        /// <summary>
        /// Create phylogenetic tree
        /// </summary>
        /// <param name="B">B is a numeric array of size [<code>NumOfBranches</code> X 2]
        /// in which every row represents
        /// a branch of the tree and it contains two pointers to the branches 
        /// or leaves nodes which are its children.</param>
        /// <param name="names">Name of leave</param>
        /// <param name="dist">a numeric array of size <code>NumOfNodes</code> with 
        /// the distances of every child node (leaf or branch) to its parent
        /// branch. For an ultrametric phylogenetic tree,  number of element is equal to 
        /// <code>NumOfBranches</code>. In ultrametric tress all
        /// the leaves are at the same location (i.e. same distance to the root).
        /// </param>
        protected void CreateTree(int[,] B, string[] names, double[] dist)
        {
            CreateTree(B, names);
            if (dist.Length == NumOfNodes)
            {
                for (int i = 0; i < dist.Length; ++i)
                {
                    if (dist[i] >= 0.0)
                    {
                        this[i].Distance = dist[i];
                    }
                }
            }
            else if (dist.Length == NumOfBranches)
            {
                for (int i = 0; i < NumOfLeaves; ++i)
                {
                    leaves[i].Distance = 0.0;
                }
                for (int i = 0; i < dist.Length; ++i)
                {
                    if (dist[i] >= 0.0)
                    {
                        branches[i].Distance = dist[i];
                    }
                }
                for (int i = 0; i < NumOfBranches; ++i)
                {
                    for (int j = 0; j < 2; ++j)
                    {
                        this[B[i, j]].Distance = dist[i] - this[B[i, j]].Distance;
                    }
                }
                branches[branches.Count - 1].Distance = 0.0; // set root at zero
            }
            else
            {
                throw new IndexOutOfRangeException("Length of dist must be same as number of nodes or branches");
            }
            invalid = true;
            Validate();
        }

        protected void CreateTree(int[,] B, string[] names)
        {
            CreateTree(B);
            for (int i = 0; i < names.Length; ++i)
            {
                if (names[i] != null && names[i].Length > 0)
                {
                    this[i].Name = names[i];
                }
            }
        }

        protected void CreateTree(int[,] B)
        {
            if (B.GetLength(1) != 2)
            {
                throw new InvalidOperationException("Length of second dimension must be 2.");
            }

            //for (int i = 0; i < B.GetLength(0); ++i)
            //{
            //    for (int j = 0; j < B.GetLength(1); ++j)
            //        Console.Write("{0} ", B[i, j]);
            //    Console.WriteLine(" ");
            //}


            // Row represent a branch
            int numOfBranches = B.GetLength(0);

            #region Create Nodes
            // Note that since only binary trees are allowed,
            // then NUMLEAVES = NUMBRANCHES + 1.
            leaves = new List<Leaf>(numOfBranches + 1);
            for (int i = 0; i < numOfBranches + 1; ++i)
            {
                leaves.Add(new Leaf(this, i.ToString()));
            }

            branches = new List<Branch>(numOfBranches);
            for (int i = 0; i < numOfBranches; ++i)
            {
                try
                {
                    branches.Add(new Branch(this, i.ToString(), this[B[i, 0]], this[B[i, 1]]));
                }
                catch (IndexOutOfRangeException e)
                {
                    throw new InvalidOperationException("Incorrect element in B at row " + i);
                }
            }
            #endregion

            #region Calculate distances
            int[] p = new int[NumOfNodes];
            for (int i = 0; i < NumOfBranches; ++i)
            {
                p[B[i, 0]] = i + 1;
                p[B[i, 1]] = i + 1;

            }
            p[NumOfNodes - 1] = NumOfBranches;
            //for (int i = 0; i < p.Length; ++i)
            //    Console.WriteLine("{0}", p[i]);
            int[] levels = new int[NumOfNodes];
            for (int i = 0; i < numOfBranches; ++i)
            {
                levels[i + NumOfLeaves] = Math.Max(levels[B[i, 0]], levels[B[i, 1]]) + 1;
            }
            for (int i = 0; i < NumOfNodes; ++i)
            {
                this[i].Distance = levels[p[i] + NumOfLeaves - 1] - levels[i];
            }

            #endregion
        }

        #endregion

        #region Properties
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                if (value != null)
                    name = value;
            }
        }

        public int NumOfBranches { get { return branches.Count; } }
        public int NumOfLeaves { get { return leaves.Count; } }
        public int NumOfNodes { get { return branches.Count + leaves.Count; } }

        /// <summary>
        /// Get the node of given index. Leaf nodes are indexed from 0
        /// to <code>NumOfLeaves</code> -1 and branch nodes are indexed from 
        /// <code>NumOfLeaves</code> to
        /// <code>NumOfLeaves</code>  + <code>NumOfBranches</code> -1.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public Node this[int i]
        {
            get
            {
                if (i < 0 || i > NumOfBranches + NumOfLeaves)
                {
                    throw new IndexOutOfRangeException();
                }
                if (i < NumOfLeaves)
                    return leaves[i];
                else
                    return branches[i - NumOfLeaves];
            }
        }

        /// <summary>
        /// Get maximun x coordiante in nodes
        /// </summary>
        public double MaxX
        {
            get
            {
                var query = from leave in leaves
                            select leave.X;

                return query.Max();
            }
        }

        /// <summary>
        /// Get maximun Y coordiante among nodes
        /// </summary>
        public double MaxY
        {
            get
            {
                var query = from leave in leaves
                            select leave.Y;

                return query.Max();
            }
        }

        #endregion 

        #region Methods
        public IEnumerator<Node> GetEnumerator()
        {
            for (int i = 0; i < NumOfNodes; ++i)
            {
                yield return this[i];
            }
        }

        /// <summary>
        /// Get index of node.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public int IndexOf(Node node)
        {
            if (node is Leaf)
                return leaves.IndexOf(node as Leaf);
            else if (node is Branch)
                return branches.IndexOf(node as Branch) + NumOfLeaves;
            else
                throw new ArgumentException("node must be leave or branch"); 
        }

        public Branch Parent(Node child)
        {
            foreach (Branch parent in branches)
            {
                if (parent.Child1 == child)
                    return parent;
                if (parent.Child2 == child)
                    return parent;
            }
            if (child is Branch)
                return child as Branch; // root node has no parent
            else
                return null; // this cannot be, only one leaf tree?
        }

        /// <summary>
        /// helper method to compute and find some features of the tree. Call this method
        /// before UI creation.
        /// </summary>
        public void Validate()
        {
            if (invalid)
            {
                #region calculate drawing coordinates
                for (int i = 0; i < NumOfNodes; ++i)
                {
                    this[i].X = this[i].Distance;
                }
                for (int i = 0; i < NumOfLeaves; ++i)
                {
                    leaves[i].Y = i;
                }
                for (int i = 0; i < NumOfBranches; ++i)
                {
                    branches[i].Y = 0;
                }

                for (int i = NumOfBranches - 1; i >= 0; --i)
                {
                    branches[i].Child1.X += branches[i].X;
                    branches[i].Child2.X += branches[i].X;
                }
                for (int i = 0; i < NumOfBranches; ++i)
                {
                    branches[i].Y = (branches[i].Child1.Y + branches[i].Child2.Y) / 2;
                }
                #endregion

                invalid = false;
            }
        }


        /// <summary>
        /// Print this on console
        /// </summary>
        public void Dump()
        {
            Console.WriteLine("Phylogenetic tree: " + Name);
            for (int i = 0; i < NumOfNodes; ++i)
            {
                Console.WriteLine("{0}. {1}", i, this[i]);
            }
        }

        /// <summary>
        /// Reorders the leaf nodes to avoid branch crossings.
        /// </summary>
        public Tree PrettyOrder()
        {
            int[] L = new int[NumOfNodes];
            for (int i = 0; i < NumOfLeaves; ++i)
                L[i] = 1;
            for (int i = 0; i < NumOfBranches; ++i)
                L[i + NumOfLeaves] = L[IndexOf(branches[i].Child1)] + L[IndexOf(branches[i].Child2)];
            double[] X = new double[NumOfNodes];
            for (int ind = NumOfBranches-1; ind > -1; --ind)
            {
                // X(tr.tree(ind,:)) = tr.dist(tr.tree(ind,:))+X(ind+numLeaves);
                X[IndexOf(branches[ind].Child1)] = branches[ind].Child1.Distance + X[ind + NumOfLeaves];
                X[IndexOf(branches[ind].Child2)] = branches[ind].Child2.Distance + X[ind + NumOfLeaves];
            }
            int[] Li = new int[NumOfNodes];
            int[] Ls = new int[NumOfNodes];
            Ls[NumOfNodes - 1] = NumOfLeaves;
            for (int ind = NumOfBranches-1; ind > -1; --ind)
            {
                int c1 = IndexOf(branches[ind].Child1);
                int c2 = IndexOf(branches[ind].Child2);
                Ls[c1] = Ls[ind + NumOfLeaves];
                Ls[c2] = Ls[ind + NumOfLeaves];
                Li[c1] = Li[ind + NumOfLeaves];
                Li[c2] = Li[ind + NumOfLeaves];
                if ((X[c2] - X[c1]) >= 0)
                {
                    Ls[c1] = Li[c1] + L[c1];
                    Li[c2] = Ls[c2] - L[c2];
                }
                else 
                {
                    Ls[c2] = Li[c2] + L[c2];
                    Li[c1] = Ls[c1] - L[c1];
                }
            }

            string[] names = new string[NumOfLeaves];
            for (int i = 0; i < NumOfLeaves; ++i)
                names[Ls[i]-1] = leaves[i].Name;
            double[] dist = new double[NumOfNodes];
            for (int i = 0; i < NumOfNodes; ++i)
                dist[i] = this[i].Distance;
            for (int i = 0; i < NumOfLeaves; ++i)
                dist[Ls[i]-1] = this[i].Distance;
            
            int[,] B = new int[NumOfBranches, 2];
            for (int i = NumOfLeaves; i < NumOfNodes; ++i)
                Ls[i] = i + 1;
            for (int i = 0; i < NumOfBranches; ++i)
            {
                int c1 = IndexOf(branches[i].Child1);
                int c2 = IndexOf(branches[i].Child2);
                B[i, 0] = Ls[c1] - 1;
                B[i, 1] = Ls[c2] - 1;
                
            }
            Tree tree = new Tree(B, names, dist);
            return tree;
        }

        #endregion
    }

    public abstract class Node
    {
        /// <summary>
        /// the distances of every child node (leaf or branch) to its parent branch
        /// </summary>
        [Description("Distance to parent node")]
        public double Distance { set; get; }
        /// <summary>
        /// Coordinate x
        /// </summary>
        public double X { set; get; }
        /// <summary>
        /// Coordinate y
        /// </summary>
        public double Y { set; get; }
        /// <summary>
        /// Name of node
        /// </summary>
        public string Name { set; get; }
        protected Tree parent;

        public Node(Tree parent, string name)
        {
            this.parent = parent;
            this.Name = name;
        }

        public override string ToString()
        {
            return String.Format("{0}: {1} (X: {2}, Y: {3}, dist: {4})",
                this.GetType().Name, Name, X, Y, Distance);
        }


    }

    public class Leaf : Node
    {
        public Leaf(Tree parent, string name)
            : base(parent, name)
        {
        }
    }

    public class Branch : Node
    {
        private Node child1;
        private Node child2;

        public Branch(Tree parent, string name, Node child1, Node child2)
            : base(parent, name)
        {
            this.child1 = child1;
            this.child2 = child2;
        }

        public Node Child1 { get { return child1; } }
        public Node Child2 { get { return child2; } }
    }

}

