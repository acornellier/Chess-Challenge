using System;
using ChessChallenge.API;
using static System.Math;

public class MyBot : IChessBot
{
    // Piece values: none, pawn, knight, bishop, rook, queen, king
    int[] _pieceValue = { 0, 100, 310, 330, 500, 1000, 0 };
    Move _bestMoveIterative;

    public Move Think(Board board, Timer timer)
    {
        var bestMoveRoot = board.GetLegalMoves()[0];
        var maxTimeMilliseconds = timer.MillisecondsRemaining / 30;
        var maxDepth = 0;

        var bestMoveRootSan = "?";
        var bestMoveIterativeSan = "?";

        ulong nodesVisited = 0;
        var bestEvalRoot = 0;
        var bestEvalIterative = 0;

        for (; maxDepth < 99; ++maxDepth)
        {
            AlphaBeta(maxDepth, 0, -1_000_000, 1_000_000);

            if (timer.MillisecondsElapsedThisTurn >= maxTimeMilliseconds)
                break;

            bestMoveRoot = _bestMoveIterative;
            bestMoveRootSan = bestMoveIterativeSan;
            bestEvalRoot = bestEvalIterative;
        }

        LogInfo(maxDepth);
        return bestMoveRoot;

        int Quiesce(int alpha, int beta)
        {
            var standPat = EvalBoard();
            if (standPat >= beta)
                return beta;

            if (standPat > alpha)
                alpha = standPat;

            foreach (var move in board.GetLegalMoves(true))
            {
                board.MakeMove(move);
                var score = -Quiesce(-beta, -alpha);
                board.UndoMove(move);

                if (score >= beta)
                    return beta;

                if (score > alpha)
                    alpha = score;
            }

            return alpha;
        }

        int AlphaBeta(int depth, int ply, int alpha, int beta)
        {
            ++nodesVisited;

            if (depth == 0)
                return Quiesce(alpha, beta);

            var moves = board.GetLegalMoves();

            // Checkmate/Stalemate
            if (moves.Length == 0)
                return board.IsInCheck() ? -999_999 : 0;

            foreach (var move in board.GetLegalMoves())
            {
                if (timer.MillisecondsElapsedThisTurn >= maxTimeMilliseconds)
                    return 888_888;

                board.MakeMove(move);
                var score = -AlphaBeta(depth - 1, ply + 1, -beta, -alpha);
                board.UndoMove(move);

                if (ply <= 1)
                    Console.WriteLine($"({ply}) {move.ToSAN(board.board)} {score}");

                if (score >= beta)
                    return beta;

                if (score > alpha)
                {
                    alpha = score;

                    if (ply == 0)
                    {
                        _bestMoveIterative = move;
                        bestMoveIterativeSan = move.ToSAN(board.board);
                        bestEvalIterative = score;
                    }
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

            var bestMoveString = $"\x1b[0mbestmove\x1b[32m {bestMoveRootSan}\x1b[37m".PadRight(31);

            var bestEvalString = $"\x1b[37meval\x1b[36m {bestEvalRoot:0.00} \x1b[37m".PadRight(29);

            var nodesString = $"\x1b[37mnodes\x1b[35m {nodesVisited}\x1b[37m".PadRight(33);

            var nodesPerSec = nodesVisited / (ulong)Max(1, timer.MillisecondsElapsedThisTurn) * 1000;
            var nodesPerSecString = $"\x1b[37mnps\x1b[34m {nodesPerSec}\x1b[37m".PadRight(32);

            Console.WriteLine(string.Join(" ", depthString, timeString, bestMoveString, bestEvalString, nodesString, nodesPerSecString));
        }
    }
}