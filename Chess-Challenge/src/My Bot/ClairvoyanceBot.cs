using ChessChallenge.API;

public class ClairvoyanceBot : IChessBot
{
    Board _board;
    int[] _pieceValues = { 0, 100, 300, 300, 500, 900, 90000 };
    int _universalDepth = 5;
    Move _bestmove;

    public Move Think(Board _board, Timer _timer)
    {
        this._board = _board;
        NegaMax(_universalDepth, -1_000_000, 1_000_000);
        return _bestmove;
    }

    int NegaMax(int depth, int alpha, int beta)
    {
        if (_board.IsInCheckmate()) return -999999;
        if (_board.IsDraw()) return -10;
        if (depth == 0) return Evaluate();
        foreach (var move in _board.GetLegalMoves())
        {
            _board.MakeMove(move);
            var score = -NegaMax(depth - 1, -beta, -alpha);
            _board.UndoMove(move);
            if (score >= beta) return beta;
            if (score > alpha)
            {
                alpha = score;
                if (depth == _universalDepth) _bestmove = move;
            }
        }

        return alpha;
    }

    int Evaluate()
    {
        int colorMult, score = 0;
        foreach (var piecelist in _board.GetAllPieceLists())
        {
            colorMult = piecelist.IsWhitePieceList == _board.IsWhiteToMove ? 1 : -1;
            score += piecelist.Count * colorMult * _pieceValues[(int)piecelist.TypeOfPieceInList];
        }

        return score;
    }
}