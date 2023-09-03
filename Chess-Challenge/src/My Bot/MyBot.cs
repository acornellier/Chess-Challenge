// #define STATS

using System.Linq;
using System.Numerics;
using ChessChallenge.API;
#if STATS
using static System.Math;
using System;
#endif

public class MyBot : IChessBot
{
    // Pesto from TyrantBot
    // Piece values: none, pawn, knight, bishop, rook, queen, king
    // Pawn, Knight, Bishop, Rook, Queen, King 
    static readonly short[] _pieceValues =
    {
        82, 337, 365, 477, 1025, 0, // Middlegame
        94, 281, 297, 512, 936, 0, // Endgame
    };

    static int _searchMaxTime;

    static readonly int[][] _unpackedPestoTables = new[]
    {
        63746705523041458768562654720m, 71818693703096985528394040064m, 75532537544690978830456252672m, 75536154932036771593352371712m,
        76774085526445040292133284352m, 3110608541636285947269332480m, 936945638387574698250991104m, 75531285965747665584902616832m,
        77047302762000299964198997571m, 3730792265775293618620982364m, 3121489077029470166123295018m, 3747712412930601838683035969m,
        3763381335243474116535455791m, 8067176012614548496052660822m, 4977175895537975520060507415m, 2475894077091727551177487608m,
        2458978764687427073924784380m, 3718684080556872886692423941m, 4959037324412353051075877138m, 3135972447545098299460234261m,
        4371494653131335197311645996m, 9624249097030609585804826662m, 9301461106541282841985626641m, 2793818196182115168911564530m,
        77683174186957799541255830262m, 4660418590176711545920359433m, 4971145620211324499469864196m, 5608211711321183125202150414m,
        5617883191736004891949734160m, 7150801075091790966455611144m, 5619082524459738931006868492m, 649197923531967450704711664m,
        75809334407291469990832437230m, 78322691297526401047122740223m, 4348529951871323093202439165m, 4990460191572192980035045640m,
        5597312470813537077508379404m, 4980755617409140165251173636m, 1890741055734852330174483975m, 76772801025035254361275759599m,
        75502243563200070682362835182m, 78896921543467230670583692029m, 2489164206166677455700101373m, 4338830174078735659125311481m,
        4960199192571758553533648130m, 3420013420025511569771334658m, 1557077491473974933188251927m, 77376040767919248347203368440m,
        73949978050619586491881614568m, 77043619187199676893167803647m, 1212557245150259869494540530m, 3081561358716686153294085872m,
        3392217589357453836837847030m, 1219782446916489227407330320m, 78580145051212187267589731866m, 75798434925965430405537592305m,
        68369566912511282590874449920m, 72396532057599326246617936384m, 75186737388538008131054524416m, 77027917484951889231108827392m,
        73655004947793353634062267392m, 76417372019396591550492896512m, 74568981255592060493492515584m, 70529879645288096380279255040m,
    }.Select(
        packedTable =>
            new BigInteger(packedTable).ToByteArray().Take(12)
                // Using search max time since it's an integer than initializes to zero and is assgined before being used again 
                .Select(square => (int)((sbyte)square * 1.461) + _pieceValues[_searchMaxTime++ % 12])
                .ToArray()
    ).ToArray();

    public Move Think(Board board, Timer timer)
    {
        var bestMoveIterative = board.GetLegalMoves()[0];
        var bestMoveRoot = bestMoveIterative;
        var maxTimeMilliseconds = timer.MillisecondsRemaining / 30;
        var maxDepth = 2;

#if STATS
        var bestMoveRootSan = "?";
        var bestMoveIterativeSan = "?";
        ulong nodesVisited = 0;
        var bestEvalRoot = 0;
        var bestEvalIterative = 0;
#endif

        for (; maxDepth < 99; ++maxDepth)
        {
            Pvs(maxDepth, 0, -1_000_000, 1_000_000);

            if (timer.MillisecondsElapsedThisTurn >= maxTimeMilliseconds)
                break;

            bestMoveRoot = bestMoveIterative;

#if STATS
            bestMoveRootSan = bestMoveIterativeSan;
            bestEvalRoot = bestEvalIterative;
#endif
        }

#if STATS
        LogInfo(maxDepth);
#endif
        return bestMoveRoot;

        int Pvs(int depth, int ply, int alpha, int beta)
        {
#if STATS
            ++nodesVisited;
#endif

            var qSearch = depth <= 0;

            if (qSearch)
            {
                var eval = EvalBoard();
                if (eval >= beta)
                    return beta;

                if (eval > alpha)
                    alpha = eval;
            }

            var moves = board.GetLegalMoves(qSearch);

            // Checkmate/Stalemate
            if (moves.Length == 0 && !qSearch)
                return board.IsInCheck() ? -999_999 : 0;

            moves = moves.OrderByDescending(move => move.CapturePieceType - move.MovePieceType).ToArray();

            var pvs = true;
            foreach (var move in moves)
            {
                if (timer.MillisecondsElapsedThisTurn >= maxTimeMilliseconds)
                    return 888_888;

                board.MakeMove(move);
                int score;
                if (pvs)
                {
                    score = -Pvs(depth - 1, ply + 1, -beta, -alpha);
                }
                else
                {
                    score = -Pvs(depth - 1, ply + 1, -alpha - 1, -alpha);
                    if (score > alpha)
                        score = -Pvs(depth - 1, ply + 1, -beta, -alpha);
                }

                board.UndoMove(move);

                if (score >= beta)
                    return beta;

                if (score > alpha)
                {
                    alpha = score;
                    pvs = false;

                    if (ply == 0)
                    {
                        bestMoveIterative = move;
#if STATS
                        bestMoveIterativeSan = move.ToSAN(board.board);
                        bestEvalIterative = score;
#endif
                    }
                }
            }

            return alpha;
        }

        int EvalBoard()
        {
            int middlegame = 0, endgame = 0, gamephase = 0, sideToMove = 2, piece;

            for (; --sideToMove >= 0; middlegame = -middlegame, endgame = -endgame)
            for (piece = -1; ++piece < 6;)
            for (var mask = board.GetPieceBitboard((PieceType)piece + 1, sideToMove > 0); mask != 0;)
            {
                // Gamephase, middlegame -> endgame
                // Multiply, then shift, then mask out 4 bits for value (0-16)
                gamephase += (0x00042110 >> (piece * 4)) & 0x0F;

                // Material and square evaluation
                var square = BitboardHelper.ClearAndGetIndexOfLSB(ref mask) ^ (56 * sideToMove);
                middlegame += _unpackedPestoTables[square][piece];
                endgame += _unpackedPestoTables[square][piece + 6];
            }

            // Tempo bonus to help with aspiration windows
            return (middlegame * gamephase + endgame * (24 - gamephase)) / 24 * (board.IsWhiteToMove ? 1 : -1) + gamephase / 2;
        }

#if STATS
        void LogInfo(int chosenDepth)
        {
            var timeString = $"\x1b[37mtime\u001b[38;5;214m {timer.MillisecondsElapsedThisTurn}ms\x1b[37m\x1b[0m".PadRight(38);

            var depthString = $"\x1b[1m\u001b[38;2;251;96;27mdepth {chosenDepth} ply\x1b[0m".PadRight(38);

            var bestMoveString = $"\x1b[0mbestmove\x1b[32m {bestMoveRootSan}\x1b[37m".PadRight(31);

            var bestEvalString = $"\x1b[37meval\x1b[36m {bestEvalRoot:0.00} \x1b[37m".PadRight(29);

            var nodesString = $"\x1b[37mnodes\x1b[35m {nodesVisited}\x1b[37m".PadRight(33);

            var nodesPerSec = nodesVisited / (ulong)Max(1, timer.MillisecondsElapsedThisTurn) * 1000;
            var nodesPerSecString = $"\x1b[37mnps\x1b[34m {nodesPerSec}\x1b[37m".PadRight(32);

            Console.WriteLine(string.Join(" ", depthString, timeString, bestMoveString, bestEvalString, nodesString, nodesPerSecString));
        }
#endif
    }
}