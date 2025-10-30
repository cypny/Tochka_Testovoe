public static class Test
{
    public static void Main()
    {
        bool allPassed = true;

        var edges1 = new List<(string, string)>
        {
            ("a", "b"),
            ("b", "c"),
            ("c", "d"),
            ("d", "A"),
            ("a", "e"),
            ("e", "f"),
            ("f", "g"),
            ("g", "h"),
            ("h", "B"),
            ("h", "C"),
            ("h", "D"),
            ("h", "E"),
            ("h", "F")
        };

        var expected1 = new[] { "B-h", "A-d", "C-h", "D-h", "E-h", "F-h" };
        allPassed &= RunTest(edges1, expected1, "Тест 1");

        var edges2 = new List<(string, string)>
        {
            ("a", "b"),
            ("b", "A"),
            ("a", "c"),
            ("c", "B"),
            ("a", "C")
        };
        var expected2 = new[] { "C-a", "A-b", "B-c" };
        allPassed &= RunTest(edges2, expected2, "Тест 2");

        var edges3 = new List<(string, string)>
        {
            ("a", "b"),
            ("b", "c"),
            ("c", "d"),
            ("d", "e"),
            ("e", "A"),
            ("e", "B"),
            ("e", "C"),
            ("e", "D"),
            ("e", "E"),
            ("e", "F"),
            ("a", "f"),
            ("f", "g"),
            ("g", "G"),
            ("g", "H"),
            ("f", "h"),
            ("h", "i"),
            ("i", "I")
        };
        var expected3 = new[] { "G-g", "H-g", "A-e", "I-i", "B-e", "C-e", "D-e", "E-e", "F-e" };
        allPassed &= RunTest(edges3, expected3, "Тест 3");

        var edges4 = new List<(string, string)>
        {
            ("a", "b"),
            ("b", "c"),
            ("c", "d"),
            ("d", "A"),
            ("a", "e"),
            ("e", "f"),
            ("f", "B")
        };
        var expected4 = new[] { "A-d", "B-f" };
        allPassed &= RunTest(edges4, expected4, "Тест 4");
        var edges5 = new List<(string, string)>
        {
            ("a", "b"),
            ("b", "c"),
            ("c", "A"),
            ("a", "d"),
            ("d", "e"),
            ("e", "f"),
            ("f", "B"),
            ("f", "B1"),
            ("f", "B2"),
            ("e", "g"),
            ("g", "C"),
            ("g", "C1"),
            ("g", "C2"),
            ("g", "C3"),
            ("e", "h"),
            ("h", "D"),
            ("h", "D1"),
            ("h", "D2"),
            ("h", "D3")
        };

        var expected5 = new[]
        {
            "B-f", "B1-f", "A-c", "C-g", "C1-g", "C2-g", "D-h", "B2-f", "D1-h", "C3-g", "D2-h", "D3-h"
        };
        allPassed &= RunTest(edges5, expected5, "Тест 5");
        Console.WriteLine($"\n{(allPassed ? "Все тесты пройдены!" : "Некоторые тесты провалены")}");
    }

    static bool RunTest(List<(string, string)> edges, string[] expected, string testName)
    {
        var result = Program.Solve(edges);
        Console.WriteLine($"Результат: [{string.Join(", ", result)}]");

        if (result.Count != expected.Length)
        {
            Console.WriteLine($"{testName}: не совпадает длина (ожидается {expected.Length}, получено {result.Count})");
            return false;
        }

        for (int i = 0; i < expected.Length; i++)
        {
            if (result[i] != expected[i])
            {
                Console.WriteLine($"{testName}: строка {i + 1} не совпадает");
                Console.WriteLine($"   Ожидалось: {expected[i]}");
                Console.WriteLine($"   Получено:  {result[i]}");
                return false;
            }
        }

        Console.WriteLine($"{testName} пройден");
        return true;
    }
}