using System;
using System.Collections.Generic;

namespace XamarinAndroidCleverPuzzle.Solver
{
    public class SolutionMove
    {
        public int move_x;
        public int move_y;
    }
    class PuzzleSolver
    {
        bool solved;
        uint[] field;
        uint field_width;
        uint field_height;
        uint field_size;
        uint cur_pos;
        ulong[] field_hashes;
        HashSet<ulong> visited_states;
        Random rnd = new Random(Guid.NewGuid().GetHashCode());
        System.Threading.Tasks.Task worker_thread = null;
        List<SolutionMove> solution;

        private uint make_pos(uint x, uint y)
        {
            return x + y * field_width;
        }

        private uint get_x_pos(uint ij)
        {
            return ij % field_width;
        }

        private uint get_y_pos(uint ij)
        {
            return (ij / field_width);
        }

        private void swap_field(uint i1, uint i2)
        {
            uint tmp = field[i1];
            field[i1] = field[i2];
            field[i2] = tmp;
        }

        private uint find_value(uint val)
        {
            for (uint i = 0; i < field_size; ++i)
            {
                if (field[i] == val)
                {
                    return i;
                }
            }
            return uint.MaxValue;
        }

        private ulong generate_ulong()
        {
            var buffer = new byte[sizeof(ulong)];
            rnd.NextBytes(buffer);
            return BitConverter.ToUInt64(buffer, 0);
        }
        private void prepare_hashtable()
        {
            field_hashes = new ulong[field_size * field_size];
            for (uint i = 0; i < field_size * field_size; ++i)
            {
                ulong rnd_val = generate_ulong();
                field_hashes[i] = rnd_val;
            }
        }

        private uint perform_move(uint cur_pos, uint tgt)
        {
            swap_field(cur_pos, tgt);
            return tgt;
        }

        private void perform_moves(ref List<uint> moves)
        {
            for (int i = 0; i < moves.Count; ++i)
            {
                cur_pos = perform_move(cur_pos, moves[i]);
            }
        }

        private ulong hash_field()
        {
            ulong result = 0;
            for (uint i = 0; i < field_size; ++i)
            {
                result ^= field_hashes[field[i] * field_size + i];
            }
            return result;
        }

        //Note: Call this BEFORE the move has been performed
        private ulong update_field_hash(ulong old_hash, uint i1, uint i2)
        {
            ulong res = old_hash;
            res ^= field_hashes[field[i1] * field_size + i1];
            res ^= field_hashes[field[i2] * field_size + i2];
            res ^= field_hashes[field[i1] * field_size + i2];
            res ^= field_hashes[field[i2] * field_size + i1];
            return res;
        }

        //Note: Call this BEFORE the move has been performed
        private uint update_future_cost(uint old_cost, uint i1, uint i2)
        {
            uint cur_l1 = l1_dist(i1) + l1_dist(i2);
            swap_field(i1, i2);
            uint new_l1 = l1_dist(i1) + l1_dist(i2);
            swap_field(i1, i2);
            return (old_cost - cur_l1 + new_l1);
        }

        uint l1_dist(uint pos)
        {
            uint val = field[pos];
            int cur_x = System.Convert.ToInt32(get_x_pos(pos));
            int cur_y = System.Convert.ToInt32(get_y_pos(pos));
            int tgt_x = System.Convert.ToInt32(get_x_pos(val));
            int tgt_y = System.Convert.ToInt32(get_y_pos(val));
            return System.Convert.ToUInt32(System.Math.Abs(cur_x - tgt_x) + System.Math.Abs(cur_y - tgt_y));
        }

        private uint compute_future_cost(bool exclude_blank = false)
        {
            uint total = 0;
            for (uint i = 0; i < field_size; ++i)
            {
                uint val = field[i];
                if (val == field_size - 1 && exclude_blank)
                {
                    continue;
                }

                total += l1_dist(i);

            }
            return total;
        }

        private void generate_moves(ref List<uint> result, uint my_pos, uint last_pos = uint.MaxValue)
        {
            result.Clear();
            uint cur_x = get_x_pos(my_pos);
            uint cur_y = get_y_pos(my_pos);
            if (cur_x > 0)
            {
                uint gen = make_pos(cur_x - 1, cur_y);
                if (gen != last_pos)
                {
                    result.Add(gen);
                }
            }
            if (cur_y > 0)
            {
                uint gen = make_pos(cur_x, cur_y - 1);
                if (gen != last_pos)
                {
                    result.Add(gen);
                }
            }
            if (cur_x + 1 < field_width)
            {
                uint gen = make_pos(cur_x + 1, cur_y);
                if (gen != last_pos)
                {
                    result.Add(gen);
                }
            }
            if (cur_y + 1 < field_height)
            {
                uint gen = make_pos(cur_x, cur_y + 1);
                if (gen != last_pos)
                {
                    result.Add(gen);
                }
            }
        }
        private void dfs_visit(ref List<uint> past_moves,
               ref List<uint> best_moves,
               ref uint best_cost,
               ref ulong visited_nodes,
               uint cur_dpt,
               ulong cur_hash,
               uint cur_l1,
               uint cur_cost,
               uint upper_bnd,
               ulong max_nodes)
        {
            if (best_cost == 0)
            {
                return;
            }
            if (visited_states.Contains(cur_hash))
            {
                return; //Redundant state.
            }
            ++visited_nodes;
            if (visited_nodes > max_nodes)
            {
                return;
            }
            visited_states.Add(cur_hash);
            //Remember this solution
            if (cur_l1 == 0)
            { //Feasible solution!
                best_moves.Clear();
                best_moves.AddRange(past_moves.ToArray());
                best_cost = 0;
                return;
            }
            else if (past_moves.Count >= 5)
            {
                uint score = 100000 + System.Convert.ToUInt32(past_moves.Count)
                                    + System.Convert.ToUInt32(70 * cur_l1);
                if (score < best_cost)
                {
                    best_cost = score;
                    best_moves.Clear();
                    best_moves.AddRange(past_moves.ToArray());
                }
            }

            List<uint> new_moves = new List<uint>();
            uint last_move = uint.MaxValue;
            if (past_moves.Count > 0)
            {
                last_move = past_moves[past_moves.Count - 1];
            }
            //Generate new moves
            generate_moves(ref new_moves, cur_pos, last_move);

            for (int i = 0; i < new_moves.Count; ++i)
            {
                uint new_cost = cur_cost + 1;
                uint new_l1 = update_future_cost(cur_l1, cur_pos, new_moves[i]);
                if (new_cost + System.Convert.ToUInt32(1.6 * new_l1) <= upper_bnd)
                {
                    past_moves.Add(new_moves[i]);
                    ulong new_hs = update_field_hash(cur_hash, cur_pos, new_moves[i]);
                    uint counter_move = cur_pos;
                    cur_pos = perform_move(cur_pos, new_moves[i]);
                    dfs_visit(ref past_moves, ref best_moves, ref best_cost, ref visited_nodes, cur_dpt + 1, new_hs, new_l1, new_cost, upper_bnd, max_nodes);
                    cur_pos = perform_move(cur_pos, counter_move);
                    past_moves.RemoveAt(past_moves.Count - 1);
                }
            }
        }

        private void ida_star()
        {
            uint cur_l1 = compute_future_cost();
            uint upper_bnd = cur_l1;
            ulong cur_hash = hash_field();
            List<uint> past_moves = new List<uint>();
            List<uint> best_moves = new List<uint>();
            visited_states = new HashSet<ulong>();
            ulong visited_nodes = 0;
            uint best_cost = uint.MaxValue;
            ulong max_nodes = 300000;
            for (uint i = 0; i < 1800; ++i)
            {
                visited_states.Clear();
                dfs_visit(ref past_moves, ref best_moves, ref best_cost, ref visited_nodes, 0, cur_hash, cur_l1, 0, upper_bnd + i, max_nodes);
                if (visited_nodes > max_nodes)
                {
                    break;
                }
                if (best_cost == 0)
                {
                    break;
                }
            }

            solution = new List<SolutionMove>();
            for (int i = 0; i < best_moves.Count; ++i)
            {
                SolutionMove step = new SolutionMove();
                uint cur_x = get_x_pos(best_moves[i]);
                uint cur_y = get_y_pos(best_moves[i]);
                step.move_x = System.Convert.ToInt32(cur_x);
                step.move_y = System.Convert.ToInt32(cur_y);

                solution.Add(step);
            }
            perform_moves(ref best_moves);
            solved = true;
        }

        public bool is_solved()
        {
            return solved;
        }

        public void run()
        {
            if (worker_thread != null)
            {
                return;
            }
            worker_thread = new System.Threading.Tasks.Task(ida_star);
            worker_thread.Start();
        }

        public List<SolutionMove> get_solution()
        {
            if (worker_thread == null)
            {
                run();
            }
            worker_thread.Wait();
            return solution;
        }

        //Remove first element.
        public void solution_pop_front()
        {
            if(solution != null && solution.Count > 0) {
                solution.RemoveAt(0);
            }
        }

        public int get_solution_value(int x, int y)
        {
            if (!is_solved())
            {
                return -1;
            }

            return System.Convert.ToInt32(field[y * System.Convert.ToInt32(field_width) + x]);
        }
        public PuzzleSolver(int[][] puzzle)
        {
            field_height = System.Convert.ToUInt32(puzzle.Length);
            field_width = System.Convert.ToUInt32(puzzle[0].Length);
            field_size = field_height * field_width;
            field = new uint[field_size];
            prepare_hashtable();

            uint i = 0;
            for (int y = 0; y < field_height; ++y)
            {
                for (int x = 0; x < field_width; ++x)
                {
                    field[i] = System.Convert.ToUInt32(puzzle[y][x]);
                    ++i;
                }
            }
            cur_pos = find_value(field_size - 1);
            solved = false;
        }
    }
}
