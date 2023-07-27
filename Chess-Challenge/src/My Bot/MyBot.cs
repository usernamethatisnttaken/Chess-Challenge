using ChessChallenge.API;

public class MyBot : IChessBot
{
    Board BOARD;
    Move DEFAULT;

    public Move Think(Board board, Timer timer)
    {
        BOARD = board;
        DEFAULT = board.GetLegalMoves()[0];
        // Move[] captures = board.GetLegalMoves(true);
        // if(captures.Length == 0) {
        //     Move[] moves = board.GetLegalMoves();
        //     return moves[0];
        // } return captures[0];

        //go thru the board
        //find enemy pieces being attacked/potential attacks -> value
        //find friendly pieces being attacked/potential attacks -> value
        //do the move with the highest value

        Move best = doAttack();

        return best;
        //return board.GetLegalMoves()[0];
    }

    private Move doAttack() {
        Move[] captures = BOARD.GetLegalMoves(true);
        int[] best = new int[]{-1, int.MinValue};

        for(int i = 0; i < captures.Length; i++) {
            int value = (int)captures[i].CapturePieceType;
            if(BOARD.SquareIsAttackedByOpponent(captures[i].TargetSquare)) {
                value -= (int)captures[i].MovePieceType;
            }
            if(value > best[1]) {
                best = new int[]{i, value};
            }
        }
        return best[0] >= 0 ? captures[best[0]] : neutral();
    }

    private Move doDefense() {
        return DEFAULT;
    }

    private Move neutral() {
        Move[] neutral = BOARD.GetLegalMoves();
        int[] best = new int[]{-1, int.MinValue};

        Square king_pos = BOARD.GetKingSquare(!BOARD.IsWhiteToMove);

        for(int i = 0; i < neutral.Length; i++) {
            int value = 0;
            if(neutral[i].MovePieceType == PieceType.Pawn) {
                value = 100;
            } else {
                if(!BOARD.SquareIsAttackedByOpponent(neutral[i].TargetSquare)) {
                    int target_val = taxicabDist(neutral[i].TargetSquare, king_pos);
                    value = target_val < taxicabDist(neutral[i].StartSquare, king_pos) ? 100 - target_val : 0;
                }
            }
            if(value > best[1]) {
                best = new int[]{i, value};
            }
        }
        return best[0] >= 0 ? neutral[best[0]] : DEFAULT;
    }

    private int taxicabDist(Square a, Square b) {
        return System.Math.Abs(a.Rank - b.Rank) + System.Math.Abs(a.File - b.File);
    }
}