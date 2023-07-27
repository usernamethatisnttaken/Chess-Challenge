using ChessChallenge.API;

public class MyBot : IChessBot
{
    Move DEFAULT;

    /** go thru the board
        find enemy pieces being attacked/potential attacks -> value
        find friendly pieces being attacked/potential attacks -> value
        do the move with the highest value
    */
    public Move Think(Board board, Timer timer)
    {
        DEFAULT = board.GetLegalMoves()[0];

        object[] atk_best = doAttack(board);
        object[] dfc_best = doDefense(board);

        if(atk_best[1].Equals(int.MinValue) && dfc_best[1].Equals(int.MinValue)) {
            return neutral(board);
        } if((int)atk_best[1] >= (int)dfc_best[1]) {
            return (Move)atk_best[0];
        } return (Move)dfc_best[0];
    }

    /// <summary>
    /// Finds the best attack with the least cost from the given board.
    /// </summary>
    private object[] doAttack(Board board) {
        Move[] captures = board.GetLegalMoves(true);
        int[] best = new int[]{-1, int.MinValue};

        for(int i = 0; i < captures.Length; i++) {
            int value = (int)captures[i].CapturePieceType;
            if(board.SquareIsAttackedByOpponent(captures[i].TargetSquare)) {
                value -= (int)captures[i].MovePieceType;
            }
            if(value > best[1]) {
                best = new int[]{i, value};
            }
        }
        return new object[]{best[0] >= 0 ? captures[best[0]] : DEFAULT, best[1]};
    }

    /// <summary>
    /// Finds the move that gets the highest priority piece out of trouble from the given board. Picks the move that either has the best
    /// chance of defending the friendly king or the best chance at attacking the enemy king.
    /// </summary>
    private object[] doDefense(Board board) {
        Move[] moves = board.GetLegalMoves();
        int[] best = new int[]{-1, int.MinValue};
        int savior_value = int.MinValue;

        Square enemy_king_pos = board.GetKingSquare(!board.IsWhiteToMove);
        Square good_king_pos = board.GetKingSquare(board.IsWhiteToMove);

        for(int i = 0; i < moves.Length; i++) {
            if(board.SquareIsAttackedByOpponent(moves[i].StartSquare)) {
                int internal_value = ((int)moves[i].MovePieceType * 100);

                if(isAttackedByEnemyKing(board, moves[i].StartSquare)) {
                    internal_value += 50 - taxicabDist(moves[i].TargetSquare, enemy_king_pos);
                } else {
                    internal_value += 50 - taxicabDist(moves[i].TargetSquare, good_king_pos);
                }

                if(board.SquareIsAttackedByOpponent(moves[i].TargetSquare)) {
                    internal_value -= 100;
                }

                if(internal_value > best[1]) {
                    best = new int[]{i, internal_value};
                    savior_value = (int)moves[i].MovePieceType;
                }
            }
        }
        return new object[]{best[0] >= 0 ? moves[best[0]] : DEFAULT, savior_value};
    }

    /// <summary>
    /// Decides what move to make from the given board when neither attacking nor defending is a great choice. Uses an (admittedly) 
    /// arbitrary system of valuing based off what might give the best advantage.
    /// </summary>
    private Move neutral(Board board) {
        Move[] moves = board.GetLegalMoves();
        int[] best = new int[]{-1, int.MinValue};

        Square enemy_king_pos = board.GetKingSquare(!board.IsWhiteToMove);

        for(int i = 0; i < moves.Length; i++) {
            int value = 0;
            if(moves[i].MovePieceType == PieceType.Pawn) {
                value = 100;
            } else {
                if(!board.SquareIsAttackedByOpponent(moves[i].TargetSquare)) {
                    int target_val = taxicabDist(moves[i].TargetSquare, enemy_king_pos);
                    value = target_val < taxicabDist(moves[i].StartSquare, enemy_king_pos) ? 100 - target_val : 0;
                }
            }
            if(value > best[1]) {
                best = new int[]{i, value};
            }
        }
        return best[0] >= 0 ? moves[best[0]] : DEFAULT;
    }

    /// <summary>
    /// Return the taxicab distance between two positions.
    /// </summary>
    private int taxicabDist(Square a, Square b) {
        return System.Math.Abs(a.Rank - b.Rank) + System.Math.Abs(a.File - b.File);
    }

    /// <summary>
    /// Is the given position attacked by the enemy king on the given board?
    /// </summary>
    private bool isAttackedByEnemyKing(Board board, Square position) {
        Square enemy_king_pos = board.GetKingSquare(!board.IsWhiteToMove);

        bool rank = (int)System.Math.Floor((double)((position.Rank + 1) / 3) - (enemy_king_pos.Rank / 3)) == 0;
        bool file = (int)System.Math.Floor((double)((position.File + 1) / 3) - (enemy_king_pos.File / 3)) == 0;

        return rank && file;
    }
}