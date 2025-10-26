using System;
using System.Collections.Generic;
using System.Linq;

namespace Solution
{
    class Program
    {
        private static readonly int[] RoomPositions = { 2, 4, 6, 8 };

        private static readonly Dictionary<char, int> Costs = new()
        {
            ['A'] = 1, ['B'] = 10, ['C'] = 100, ['D'] = 1000
        };

        public static readonly Dictionary<int, string[]> GoalRooms = new()
        {
            [2] = new[] { "AA", "BB", "CC", "DD" }, [4] = new[] { "AAAA", "BBBB", "CCCC", "DDDD" }
        };

        static int Solve(List<string> lines)
        {
            var initial = ParseInput(lines);
            return Dijkstra(initial);
        }

        private static State ParseInput(List<string> lines)
        {
            var corridor = new string('.', 11);
            var depth = lines.Count - 3;
            var rooms = new string[4];

            for (var i = 0; i < 4; i++)
            {
                var roomChars = new char[depth];
                for (var j = 0; j < depth; j++)
                {
                    roomChars[j] = lines[2 + j][3 + 2 * i];
                }

                rooms[i] = new string(roomChars);
            }

            return new State(corridor, rooms, depth);
        }

        private static int Dijkstra(State initial)
        {
            var queue = new PriorityQueue<State, int>();
            var visited = new Dictionary<string, int>();

            queue.Enqueue(initial, 0);
            visited[initial.Key] = 0;

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                var currentEnergy = visited[current.Key];

                if (current.IsGoal)
                    return currentEnergy;

                foreach (var (nextState, moveCost) in MoveGenerator.GenerateMoves(current))
                {
                    var newEnergy = currentEnergy + moveCost;
                    if (!visited.TryGetValue(nextState.Key, out var existingEnergy) || newEnergy < existingEnergy)
                    {
                        visited[nextState.Key] = newEnergy;
                        queue.Enqueue(nextState, newEnergy);
                    }
                }
            }

            return -1;
        }

        static void Main()
        {
            //RunTests();
        
            var lines = new List<string>();
            string line;

            while ((line = Console.ReadLine()) != null)
            {
                lines.Add(line);
            }

            if (lines.Count > 0)
            {
                var result = Solve(lines);
                Console.WriteLine(result);
            }
        }

        private static void RunTests()
        {
            Console.Error.WriteLine("Running tests...");
            
            var test1 = new List<string>
            {
                "#############",
                "#...........#",
                "###B#C#B#D###",
                "  #A#D#C#A#",
                "  #########"
            };

            var result1 = Solve(test1);
            var expected1 = 12521;
            Console.Error.WriteLine($"Test 1 (Depth 2): {result1} (expected {expected1}) - {(result1 == expected1 ? "PASS" : "FAIL")}");
            
            var test2 = new List<string>
            {
                "#############",
                "#...........#",
                "###B#C#B#D###",
                "  #D#C#B#A#",
                "  #D#B#A#C#",
                "  #A#D#C#A#",
                "  #########"
            };

            var result2 = Solve(test2);
            var expected2 = 44169;
            Console.Error.WriteLine($"Test 2 (Depth 4): {result2} (expected {expected2}) - {(result2 == expected2 ? "PASS" : "FAIL")}");

            Console.Error.WriteLine("Tests completed.");
        }

    }

    public class State
    {
        public string Corridor { get; }
        public string[] Rooms { get; }
        public int Depth { get; }
        public string Key { get; }

        public State(string corridor, string[] rooms, int depth)
        {
            Corridor = corridor;
            Rooms = rooms;
            Depth = depth;
            Key = corridor + string.Join("", Rooms);
        }

        public bool IsGoal
        {
            get
            {
                if (!Program.GoalRooms.TryGetValue(Depth, out var goalRooms))
                {
                    for (var i = 0; i < 4; i++)
                    {
                        var target = (char)('A' + i);
                        if (Rooms[i] != new string(target, Depth))
                            return false;
                    }

                    return true;
                }

                for (var i = 0; i < 4; i++)
                {
                    if (Rooms[i] != goalRooms[i])
                        return false;
                }

                return true;
            }
        }
    }

    public static class MoveGenerator
    {
        private static readonly int[] RoomPositions = { 2, 4, 6, 8 };

        private static readonly Dictionary<char, int> Costs = new()
        {
            ['A'] = 1, ['B'] = 10, ['C'] = 100, ['D'] = 1000
        };

        public static IEnumerable<(State, int)> GenerateMoves(State state)
        {
            foreach (var move in GenerateRoomToCorridorMoves(state))
                yield return move;
            foreach (var move in GenerateCorridorToRoomMoves(state))
                yield return move;
        }

        private static IEnumerable<(State, int)> GenerateRoomToCorridorMoves(State state)
        {
            for (var roomIndex = 0; roomIndex < 4; roomIndex++)
            {
                for (var depth = 0; depth < state.Depth; depth++)
                {
                    if (!CanMoveFromRoom(state, roomIndex, depth))
                        continue;

                    var amphipod = state.Rooms[roomIndex][depth];
                    foreach (var corridorPos in GetValidCorridorPositions())
                    {
                        if (!IsPathClearRoomToCorridor(state, roomIndex, corridorPos))
                            continue;

                        var steps = CalculateStepsFromRoom(roomIndex, depth, corridorPos);
                        var cost = steps * Costs[amphipod];
                        var newState = CreateRoomToCorridorState(state, roomIndex, depth, corridorPos);
                        yield return (newState, cost);
                    }
                }
            }
        }

        private static IEnumerable<(State, int)> GenerateCorridorToRoomMoves(State state)
        {
            for (var corridorPos = 0; corridorPos < 11; corridorPos++)
            {
                if (state.Corridor[corridorPos] == '.')
                    continue;

                var amphipod = state.Corridor[corridorPos];
                var targetRoom = amphipod - 'A';

                if (targetRoom < 0 || targetRoom > 3) continue;
                if (!CanMoveToRoom(state, targetRoom) || !IsPathClearCorridorToRoom(state, corridorPos, targetRoom))
                    continue;

                var depth = GetTargetDepth(state, targetRoom);
                if (depth == -1) continue;

                var steps = CalculateStepsToRoom(corridorPos, targetRoom, depth);
                var cost = steps * Costs[amphipod];
                var newState = CreateCorridorToRoomState(state, corridorPos, targetRoom, depth);
                yield return (newState, cost);
            }
        }

        private static bool CanMoveFromRoom(State state, int roomIndex, int depth)
        {
            if (state.Rooms[roomIndex][depth] == '.')
                return false;

            for (var i = 0; i < depth; i++)
            {
                if (state.Rooms[roomIndex][i] != '.')
                    return false;
            }

            var amphipod = state.Rooms[roomIndex][depth];
            var targetRoom = amphipod - 'A';

            if (roomIndex == targetRoom)
            {
                for (var i = depth; i < state.Depth; i++)
                {
                    var other = state.Rooms[roomIndex][i];
                    if (other != '.' && other - 'A' != roomIndex)
                        return true;
                }

                return false;
            }

            return true;
        }

        private static bool CanMoveToRoom(State state, int roomIndex)
        {
            for (var i = 0; i < state.Depth; i++)
            {
                var c = state.Rooms[roomIndex][i];
                if (c != '.' && c - 'A' != roomIndex)
                    return false;
            }

            return true;
        }

        private static IEnumerable<int> GetValidCorridorPositions()
        {
            return Enumerable.Range(0, 11).Where(i => !RoomPositions.Contains(i));
        }

        private static bool IsPathClearRoomToCorridor(State state, int roomIndex, int corridorPos)
        {
            var roomCorridorPos = RoomPositions[roomIndex];
            var start = Math.Min(roomCorridorPos, corridorPos);
            var end = Math.Max(roomCorridorPos, corridorPos);

            for (var i = start; i <= end; i++)
            {
                if (i == roomCorridorPos) continue;
                if (i < 0 || i >= state.Corridor.Length) return false;
                if (state.Corridor[i] != '.') return false;
            }

            return true;
        }

        private static bool IsPathClearCorridorToRoom(State state, int corridorPos, int roomIndex)
        {
            var roomCorridorPos = RoomPositions[roomIndex];
            var start = Math.Min(roomCorridorPos, corridorPos);
            var end = Math.Max(roomCorridorPos, corridorPos);

            for (var i = start; i <= end; i++)
            {
                if (i == corridorPos) continue;
                if (i < 0 || i >= state.Corridor.Length) return false;
                if (state.Corridor[i] != '.') return false;
            }

            return true;
        }

        private static int CalculateStepsFromRoom(int roomIndex, int depth, int corridorPos)
        {
            var roomCorridorPos = RoomPositions[roomIndex];
            return depth + 1 + Math.Abs(roomCorridorPos - corridorPos);
        }

        private static int CalculateStepsToRoom(int corridorPos, int roomIndex, int depth)
        {
            var roomCorridorPos = RoomPositions[roomIndex];
            return Math.Abs(corridorPos - roomCorridorPos) + depth + 1;
        }

        private static int GetTargetDepth(State state, int roomIndex)
        {
            for (var d = state.Depth - 1; d >= 0; d--)
            {
                if (state.Rooms[roomIndex][d] == '.')
                    return d;
            }

            return -1;
        }

        private static State CreateRoomToCorridorState(State state, int roomIndex, int depth, int corridorPos)
        {
            var newCorridor = state.Corridor.ToCharArray();
            newCorridor[corridorPos] = state.Rooms[roomIndex][depth];

            var newRooms = (string[])state.Rooms.Clone();
            var roomChars = newRooms[roomIndex].ToCharArray();
            roomChars[depth] = '.';
            newRooms[roomIndex] = new string(roomChars);

            return new State(new string(newCorridor), newRooms, state.Depth);
        }

        private static State CreateCorridorToRoomState(State state, int corridorPos, int roomIndex, int depth)
        {
            var newCorridor = state.Corridor.ToCharArray();
            newCorridor[corridorPos] = '.';

            var newRooms = (string[])state.Rooms.Clone();
            var roomChars = newRooms[roomIndex].ToCharArray();
            roomChars[depth] = state.Corridor[corridorPos];
            newRooms[roomIndex] = new string(roomChars);

            return new State(new string(newCorridor), newRooms, state.Depth);
        }
    }
}