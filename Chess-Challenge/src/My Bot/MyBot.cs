using System.Linq;
using ChessChallenge.API;

public class MyBot : IChessBot
{
    // Piece values: pawn, knight, bishop, rook, queen, king
    int[] _pieceValue = { 0, 100, 310, 330, 500, 1000, 1_000_000 };

    public Move Think(Board board, Timer timer)
    {
        var moveToPlay = board.GetLegalMoves()[0];
        var maxDepth = 3;
        NegaMax(maxDepth);
        return moveToPlay;

        int NegaMax(int depth)
        {
            if (board.IsInCheckmate())
                return -1_000_000;

            if (board.IsDraw())
                return -10;

            if (depth == 0) return EvalBoard();

            var max = int.MinValue;
            foreach (var move in board.GetLegalMoves())
            {
                board.MakeMove(move);
                var score = -NegaMax(depth - 1);
                if (score > max)
                {
                    max = score;
                    if (depth == maxDepth)
                        moveToPlay = move;
                }

                board.UndoMove(move);
            }

            return max;
        }

        int EvalBoard()
        {
            var isWhite = board.IsWhiteToMove;

            return board.GetAllPieceLists().Sum(
                pieceList =>
                    pieceList.Sum(
                        piece => (piece.IsWhite == isWhite ? 1 : -1) * _pieceValue[(int)piece.PieceType]
                    )
            );
        }
    }
}