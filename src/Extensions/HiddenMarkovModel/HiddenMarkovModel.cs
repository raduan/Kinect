// Accord Statistics Library
// The Accord.NET Framework
// http://accord-net.origo.ethz.ch
//
// Copyright © César Souza, 2009-2011
// Copyright © Guilherme Pedroso, 2009
//

using System;
using Accord.Math;
using Accord.Statistics.Distributions.Univariate;
using Accord.Statistics.Models.Markov.Topology;

namespace Accord.Statistics.Models.Markov
{
    /// <summary>
    ///   Discrete-density Hidden Markov Model.
    /// </summary>
    /// 
    /// <remarks>
    /// <para>
    ///   Hidden Markov Models (HMM) are stochastic methods to model temporal and sequence
    ///   data. They are especially known for their application in temporal pattern recognition
    ///   such as speech, handwriting, gesture recognition, part-of-speech tagging, musical
    ///   score following, partial discharges and bioinformatics.</para>
    /// <para>
    ///   Dynamical systems of discrete nature assumed to be governed by a Markov chain emits
    ///   a sequence of observable outputs. Under the Markov assumption, it is also assumed that
    ///   the latest output depends only on the current state of the system. Such states are often
    ///   not known from the observer when only the output values are observable.</para>
    ///   
    /// <para>
    ///   Hidden Markov Models attempt to model such systems and allow, among other things,
    ///   <list type="number">
    ///     <item><description>
    ///       To infer the most likely sequence of states that produced a given output sequence,</description></item>
    ///     <item><description>
    ///       Infer which will be the most likely next state (and thus predicting the next output),</description></item>
    ///     <item><description>
    ///       Calculate the probability that a given sequence of outputs originated from the system
    ///       (allowing the use of hidden Markov models for sequence classification).</description></item>
    ///     </list></para>
    ///     
    /// <para>     
    ///   The “hidden” in Hidden Markov Models comes from the fact that the observer does not
    ///   know in which state the system may be in, but has only a probabilistic insight on where
    ///   it should be.</para>
    ///   
    /// <para>
    ///   References:
    ///   <list type="bullet">
    ///     <item><description>
    ///       http://en.wikipedia.org/wiki/Hidden_Markov_model</description></item>
    ///     <item><description>
    ///       http://www.shokhirev.com/nikolai/abc/alg/hmm/hmm.html</description></item>
    ///     <item><description>
    ///       P396-397 “Spoken Language Processing” by X. Huang </description></item>
    ///     <item><description>
    ///       Dawei Shen. Some mathematics for HMMs, 2008. Available in:
    ///       http://courses.media.mit.edu/2010fall/mas622j/ProblemSets/ps4/tutorial.pdf</description></item>
    ///     <item><description>
    ///       http://www.stanford.edu/class/cs262/presentations/lecture7.pdf</description></item>
    ///     <item><description>
    ///       http://cs.oberlin.edu/~jdonalds/333/lecture11.html</description></item>
    ///   </list></para>
    /// </remarks>
    /// 
    /// <seealso cref="ContinuousHiddenMarkovModel">Continuous-density Hidden Markov Model.</seealso>
    /// 
    /// <example>
    ///   <code>
    ///   // We will try to create a Hidden Markov Model which
    ///   //  can detect if a given sequence starts with a zero
    ///   //  and has any number of ones after that.
    ///   int[][] sequences = new int[][] 
    ///   {
    ///       new int[] { 0,1,1,1,1,0,1,1,1,1 },
    ///       new int[] { 0,1,1,1,0,1,1,1,1,1 },
    ///       new int[] { 0,1,1,1,1,1,1,1,1,1 },
    ///       new int[] { 0,1,1,1,1,1         },
    ///       new int[] { 0,1,1,1,1,1,1       },
    ///       new int[] { 0,1,1,1,1,1,1,1,1,1 },
    ///       new int[] { 0,1,1,1,1,1,1,1,1,1 },
    ///   };
    ///   
    ///   // Creates a new Hidden Markov Model with 3 states for
    ///   //  an output alphabet of two characters (zero and one)
    ///   HiddenMarkovModel hmm = new HiddenMarkovModel(3, 2);
    ///   
    ///   // Try to fit the model to the data until the difference in
    ///   //  the average log-likelihood changes only by as little as 0.0001
    ///   var teacher = new BaumWelchLearning(hmm) { Tolerance = 0.0001, Iterations = 0 };
    ///   double ll = teacher.Run(sequences);
    ///   
    ///   // Calculate the probability that the given
    ///   //  sequences originated from the model
    ///   double l1 = hmm.Evaluate(new int[] { 0, 1 });       // 0.999
    ///   double l2 = hmm.Evaluate(new int[] { 0, 1, 1, 1 }); // 0.916
    ///   
    ///   // Sequences which do not start with zero have much lesser probability.
    ///   double l3 = hmm.Evaluate(new int[] { 1, 1 });       // 0.000
    ///   double l4 = hmm.Evaluate(new int[] { 1, 0, 0, 0 }); // 0.000
    ///   
    ///   // Sequences which contains few errors have higher probabability
    ///   //  than the ones which do not start with zero. This shows some
    ///   //  of the temporal elasticity and error tolerance of the HMMs.
    ///   double l5 = hmm.Evaluate(new int[] { 0, 1, 0, 1, 1, 1, 1, 1, 1 }); // 0.034
    ///   double l6 = hmm.Evaluate(new int[] { 0, 1, 1, 1, 1, 1, 1, 0, 1 }); // 0.034
    ///   </code>
    /// </example>
    /// 
    [Serializable]
    public class HiddenMarkovModel : HiddenMarkovModelBase, IHiddenMarkovModel
    {
        //Size of vocabulary

        // Model is defined as M = (A, B, pi)
        // Parameters (A, pi) are defined in base class
        private readonly double[,] B; // emission probabilities
        private readonly int symbols;

        #region Constructors

        /// <summary>
        ///   Constructs a new Hidden Markov Model.
        /// </summary>
        /// <param name="topology">
        ///   A <see cref="Topology"/> object specifying the initial values of the matrix of transition 
        ///   probabilities <c>A</c> and initial state probabilities <c>pi</c> to be used by this model.
        /// </param>
        /// <param name="emissions">The emissions matrix B for this model.</param>
        public HiddenMarkovModel(ITopology topology, double[,] emissions)
            : base(topology)
        {
            B = emissions;
        }

        /// <summary>
        ///   Constructs a new Hidden Markov Model.
        /// </summary>
        /// <param name="topology">
        ///   A <see cref="Topology"/> object specifying the initial values of the matrix of transition 
        ///   probabilities <c>A</c> and initial state probabilities <c>pi</c> to be used by this model.
        /// </param>
        /// <param name="symbols">The number of output symbols used for this model.</param>
        public HiddenMarkovModel(ITopology topology, int symbols)
            : base(topology)
        {
            this.symbols = symbols;

            // Initialize B with uniform probabilities
            B = new double[States,symbols];
            for (int i = 0; i < States; i++)
                for (int j = 0; j < symbols; j++)
                    B[i, j] = 1.0/symbols;
        }

        /// <summary>
        ///   Constructs a new Hidden Markov Model.
        /// </summary>
        /// <param name="transitions">The transitions matrix A for this model.</param>
        /// <param name="emissions">The emissions matrix B for this model.</param>
        /// <param name="initial">The initial state probabilities for this model.</param>
        public HiddenMarkovModel(double[,] transitions, double[,] emissions, double[] initial)
            : this(new Custom(transitions, initial), emissions)
        {
        }

        /// <summary>
        ///   Constructs a new Hidden Markov Model.
        /// </summary>
        /// <param name="states">The number of states for this model.</param>
        /// <param name="symbols">The number of output symbols used for this model.</param>
        public HiddenMarkovModel(int states, int symbols)
            : this(new Ergodic(states), symbols)
        {
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///   Gets the number of symbols in the alphabet of this model.
        /// </summary>
        public int Symbols
        {
            get { return symbols; }
        }

        /// <summary>
        ///   Gets the Emission matrix (B) for this model.
        /// </summary>
        public double[,] Emissions
        {
            get { return B; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///   Calculates the most likely sequence of hidden states
        ///   that produced the given observation sequence.
        /// </summary>
        /// <remarks>
        ///   Decoding problem. Given the HMM M = (A, B, pi) and  the observation sequence 
        ///   O = {o1,o2, ..., oK}, calculate the most likely sequence of hidden states Si
        ///   that produced this observation sequence O. This can be computed efficiently
        ///   using the Viterbi algorithm.
        /// </remarks>
        /// <param name="observations">A sequence of observations.</param>
        /// <param name="probability">The state optimized probability.</param>
        /// <returns>The sequence of states that most likely produced the sequence.</returns>
        public int[] Decode(int[] observations, out double probability)
        {
            return Decode(observations, false, out probability);
        }

        /// <summary>
        ///   Calculates the most likely sequence of hidden states
        ///   that produced the given observation sequence.
        /// </summary>
        /// <remarks>
        ///   Decoding problem. Given the HMM M = (A, B, pi) and  the observation sequence 
        ///   O = {o1,o2, ..., oK}, calculate the most likely sequence of hidden states Si
        ///   that produced this observation sequence O. This can be computed efficiently
        ///   using the Viterbi algorithm.
        /// </remarks>
        /// <param name="observations">A sequence of observations.</param>
        /// <param name="probability">The state optimized probability.</param>
        /// <param name="logarithm">True to return the log-likelihood, false to return
        /// the likelihood. Default is false (default is to return the likelihood).</param>
        /// <returns>The sequence of states that most likely produced the sequence.</returns>
        public int[] Decode(int[] observations, bool logarithm, out double probability)
        {
            if (observations == null)
                throw new ArgumentNullException("observations");

            if (observations.Length == 0)
            {
                probability = 0.0;
                return new int[0];
            }


            // Viterbi-forward algorithm.
            int T = observations.Length;
            int states = States;
            int minState;
            double minWeight;
            double weight;

            double[] pi = Probabilities;
            double[,] A = Transitions;

            var s = new int[states,T];
            var a = new double[states,T];


            // Base
            for (int i = 0; i < states; i++)
                a[i, 0] = -System.Math.Log(pi[i]) - System.Math.Log(B[i, observations[0]]);

            // Induction
            for (int t = 1; t < T; t++)
            {
                int observation = observations[t];

                for (int j = 0; j < states; j++)
                {
                    minState = 0;
                    minWeight = a[0, t - 1] - System.Math.Log(A[0, j]);

                    for (int i = 1; i < states; i++)
                    {
                        weight = a[i, t - 1] - System.Math.Log(A[i, j]);

                        if (weight < minWeight)
                        {
                            minState = i;
                            minWeight = weight;
                        }
                    }

                    a[j, t] = minWeight - System.Math.Log(B[j, observation]);
                    s[j, t] = minState;
                }
            }


            // Find minimum value for time T-1
            minState = 0;
            minWeight = a[0, T - 1];

            for (int i = 1; i < states; i++)
            {
                if (a[i, T - 1] < minWeight)
                {
                    minState = i;
                    minWeight = a[i, T - 1];
                }
            }


            // Trackback
            var path = new int[T];
            path[T - 1] = minState;

            for (int t = T - 2; t >= 0; t--)
                path[t] = s[path[t + 1], t + 1];


            // Returns the sequence probability as an out parameter
            probability = logarithm ? -minWeight : System.Math.Exp(-minWeight);

            // Returns the most likely (Viterbi path) for the given sequence
            return path;
        }


        /// <summary>
        ///   Calculates the probability that this model has generated the given sequence.
        /// </summary>
        /// <remarks>
        ///   Evaluation problem. Given the HMM  M = (A, B, pi) and  the observation
        ///   sequence O = {o1, o2, ..., oK}, calculate the probability that model
        ///   M has generated sequence O. This can be computed efficiently using the
        ///   either the Viterbi or the Forward algorithms.
        /// </remarks>
        /// <param name="observations">
        ///   A sequence of observations.
        /// </param>
        /// <returns>
        ///   The probability that the given sequence has been generated by this model.
        /// </returns>
        public double Evaluate(int[] observations)
        {
            return Evaluate(observations, false);
        }

        /// <summary>
        ///   Calculates the probability that this model has generated the given sequence.
        /// </summary>
        /// <remarks>
        ///   Evaluation problem. Given the HMM  M = (A, B, pi) and  the observation
        ///   sequence O = {o1, o2, ..., oK}, calculate the probability that model
        ///   M has generated sequence O. This can be computed efficiently using the
        ///   either the Viterbi or the Forward algorithms.
        /// </remarks>
        /// <param name="observations">
        ///   A sequence of observations.
        /// </param>
        /// <param name="logarithm">
        ///   True to return the log-likelihood, false to return
        ///   the likelihood. Default is false.
        /// </param>
        /// <returns>
        ///   The probability that the given sequence has been generated by this model.
        /// </returns>
        public double Evaluate(int[] observations, bool logarithm)
        {
            if (observations == null)
                throw new ArgumentNullException("observations");

            if (observations.Length == 0)
                return 0.0;

            // Forward algorithm
            double logLikelihood;

            // Compute forward probabilities
            ForwardBackwardAlgorithm.Forward(this, observations, out logLikelihood);

            // Return the sequence probability
            return logarithm ? logLikelihood : System.Math.Exp(logLikelihood);
        }


        /// <summary>
        ///   Predicts next observations occurring after a given observation sequence.
        /// </summary>
        public int[] Predict(int[] observations, int next, out double probability)
        {
            double[][] probabilities;
            return Predict(observations, next, false, out probability, out probabilities);
        }

        /// <summary>
        ///   Predicts next observations occurring after a given observation sequence.
        /// </summary>
        public int[] Predict(int[] observations, int next)
        {
            double probability;
            double[][] probabilities;
            return Predict(observations, next, false, out probability, out probabilities);
        }

        /// <summary>
        ///   Predicts next observations occurring after a given observation sequence.
        /// </summary>
        public int[] Predict(int[] observations, int next, out double[][] probabilities)
        {
            double probability;
            return Predict(observations, next, false, out probability, out probabilities);
        }

        /// <summary>
        ///   Predicts the next observation occurring after a given observation sequence.
        /// </summary>
        public int Predict(int[] observations, out double[] probabilities)
        {
            double[][] prob;
            double probability;
            int prediction = Predict(observations, 1, false, out probability, out prob)[0];
            probabilities = prob[0];
            return prediction;
        }

        /// <summary>
        ///   Predicts the next observations occurring after a given observation sequence.
        /// </summary>
        public int[] Predict(int[] observations, int next, bool logarithm,
                             out double probability, out double[][] probabilities)
        {
            int states = States;
            int T = next;
            double[,] A = Transitions;

            double logLikelihood;
            var prediction = new int[next];
            probabilities = new double[next][];


            // Compute forward probabilities for the given observation sequence.
            double[,] fw0 = ForwardBackwardAlgorithm.Forward(this, observations, out logLikelihood);

            // Create a matrix to store the future probabilities for the prediction
            // sequence and copy the latest forward probabilities on its first row.
            var fwd = new double[T + 1,States];


            // 1. Initialization
            for (int i = 0; i < States; i++)
                fwd[0, i] = fw0[observations.Length - 1, i];


            // 2. Induction
            for (int t = 0; t < T; t++)
            {
                var weights = new double[Symbols];
                for (int s = 0; s < Symbols; s++)
                {
                    for (int i = 0; i < states; i++)
                    {
                        double sum = 0.0;
                        for (int j = 0; j < states; j++)
                            sum += fwd[t, j]*A[j, i];
                        fwd[t + 1, i] = sum*B[i, s];

                        weights[s] += fwd[t + 1, i];
                    }

                    weights[s] /= System.Math.Exp(logLikelihood);

                    if (weights[s] != 0) // Scaling
                    {
                        for (int i = 0; i < states; i++)
                            fwd[t + 1, i] /= weights[s];
                    }
                }

                // Select most probable symbol
                double maxWeight = weights[0];
                double sumWeight = maxWeight;
                for (int i = 1; i < weights.Length; i++)
                {
                    if (weights[i] > maxWeight)
                    {
                        maxWeight = weights[i];
                        prediction[t] = i;
                    }

                    sumWeight += weights[i];
                }

                // Compute and save symbol probabilities
                for (int i = 0; i < weights.Length; i++)
                    weights[i] /= sumWeight;
                probabilities[t] = weights;

                // Recompute log-likelihood
                logLikelihood = System.Math.Log(maxWeight);
            }

            // Returns the sequence probability as an out parameter
            probability = logarithm ? logLikelihood : System.Math.Exp(logLikelihood);

            return prediction;
        }


        /// <summary>
        ///   Converts this <see cref="HiddenMarkovModel">Discrete density Hidden Markov Model</see>
        ///   into a <see cref="ContinuousHiddenMarkovModel">Continuous density model</see>.
        /// </summary>
        public ContinuousHiddenMarkovModel ToContinuousModel()
        {
            var transitions = (double[,]) Transitions.Clone();
            var probabilities = (double[]) Probabilities.Clone();

            var emissions = new GeneralDiscreteDistribution[States];
            for (int i = 0; i < emissions.Length; i++)
                emissions[i] = new GeneralDiscreteDistribution(Emissions.GetRow(i));

            return new ContinuousHiddenMarkovModel(transitions, emissions, probabilities);
        }

        /// <summary>
        ///   Converts this <see cref="HiddenMarkovModel">Discrete density Hidden Markov Model</see>
        ///   to a <see cref="ContinuousHiddenMarkovModel">Continuous density model</see>.
        /// </summary>
        public static explicit operator ContinuousHiddenMarkovModel(HiddenMarkovModel model)
        {
            return model.ToContinuousModel();
        }

        #endregion

        #region IHiddenMarkovModel implementation

        int[] IHiddenMarkovModel.Decode(Array sequence, out double probability)
        {
            return Decode((int[]) sequence, out probability);
        }

        int[] IHiddenMarkovModel.Decode(Array sequence, bool logarithm, out double probability)
        {
            return Decode((int[]) sequence, logarithm, out probability);
        }

        double IHiddenMarkovModel.Evaluate(Array sequence)
        {
            return Evaluate((int[]) sequence);
        }

        double IHiddenMarkovModel.Evaluate(Array sequence, bool logarithm)
        {
            return Evaluate((int[]) sequence, logarithm);
        }

        #endregion

        //---------------------------------------------

        //---------------------------------------------

        //---------------------------------------------

        //---------------------------------------------
    }
}