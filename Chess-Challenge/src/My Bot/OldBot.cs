using System;
using ChessChallenge.API;
using static System.Math;

public class OldBot : IChessBot
{
    // Piece values: none, pawn, knight, bishop, rook, queen, king
    int[] _pieceValue = { 0, 100, 310, 330, 500, 1000, 0 };

    public Move Think(Board board, Timer timer)
    {
        var moveToPlay = board.GetLegalMoves()[0];
        var maxTimeMilliseconds = timer.MillisecondsRemaining / 30;
        var maxDepth = 0;

        ulong nodesVisited = 0;
        var bestEval = 0;

        for (; maxDepth < 99; ++maxDepth)
        {
            var eval = AlphaBeta(maxDepth, 0, -1_000_000, 1_000_000);

            if (timer.MillisecondsElapsedThisTurn >= maxTimeMilliseconds)
                break;

            bestEval = eval;
        }

        LogInfo(maxDepth);
        return moveToPlay;

        int AlphaBeta(int depth, int ply, int alpha, int beta)
        {
            ++nodesVisited;

            if (board.IsInCheckmate())
                return -999_999;

            if (board.IsDraw())
                return -10;

            if (depth == 0)
                return EvalBoard();

            foreach (var move in board.GetLegalMoves())
            {
                if (timer.MillisecondsElapsedThisTurn >= maxTimeMilliseconds)
                    return 999_999;

                board.MakeMove(move);
                var score = -AlphaBeta(depth - 1, ply + 1, -beta, -alpha);
                board.UndoMove(move);

                if (score >= beta)
                    return beta;

                if (score > alpha)
                {
                    alpha = score;

                    if (ply == 0)
                        moveToPlay = move;
                }
            }

            return alpha;
        }

        int EvalBoard()
        {
            var isWhite = board.IsWhiteToMove;

            var score = 0;
            foreach (var pieceList in board.GetAllPieceLists())
            foreach (var piece in pieceList)
                score += (piece.IsWhite == isWhite ? 1 : -1) * _pieceValue[(int)piece.PieceType];
            return score;
        }

        void LogInfo(int chosenDepth)
        {
            var timeString = $"\x1b[37mtime\u001b[38;5;214m {timer.MillisecondsElapsedThisTurn}ms\x1b[37m\x1b[0m".PadRight(38);

            var depthString = $"\x1b[1m\u001b[38;2;251;96;27mdepth {chosenDepth} ply\x1b[0m".PadRight(38);

            var bestMoveString = $"\x1b[0mbestmove\x1b[32m {moveToPlay}\x1b[37m".PadRight(38);

            var bestEvalString = $"\x1b[37meval\x1b[36m {bestEval:0.00} \x1b[37m".PadRight(29);

            var nodesString = $"\x1b[37mnodes\x1b[35m {nodesVisited}\x1b[37m".PadRight(33);

            var nodesPerSec = nodesVisited / (ulong)Max(1, timer.MillisecondsElapsedThisTurn) * 1000;
            var nodesPerSecString = $"\x1b[37mnps\x1b[34m {nodesPerSec}\x1b[37m".PadRight(32);

            Console.WriteLine(string.Join(" ", depthString, timeString, bestMoveString, bestEvalString, nodesString, nodesPerSecString));
        }
    }
}