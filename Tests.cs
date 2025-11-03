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
        var expected3 = new[] {"A-e", "G-g", "H-g", "B-e", "C-e","I-i", "D-e", "E-e", "F-e" };
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
            ("f", "C"),     
            ("f", "D"),

            ("e", "g"),
            ("g", "E"),
            ("g", "F"), 
            ("g", "G"),
            ("g", "H"),

            ("e", "h"),
            ("h", "I"),
            ("h", "J"),
            ("h", "K"),
            ("h", "L")
        };

        var expected5 = new[]
        {
            "B-f",
            "C-f",
            "A-c",
            "E-g",
            "F-g",
            "G-g",
            "I-h",
            "D-f",
            "J-h",
            "H-g",
            "K-h",
            "L-h"
        };
        allPassed &= RunTest(edges5, expected5, "Тест 5");
        var edges6 = new List<(string, string)>
        {
            ("a", "b"),
            ("a", "c"),
            ("b", "D"),
            ("c", "D"),
        };

        var expected6 = new[] { "D-b", "D-c"};
        allPassed &= RunTest(edges6, expected6, "Тест 6");
        var edges7 = new List<(string, string)>
        {
            ("a", "b"),
            ("b", "c"),
            ("c", "d"),
            ("b", "A"),
            ("c", "B"),
            ("d", "C"),
        };
        var expected7 = new[] { "A-b", "B-c","C-d"};
        allPassed &= RunTest(edges7, expected7, "Тест 7");
        var edges8 = new List<(string, string)>
        {
            ("a", "b"),
            ("b", "c"),
            ("c", "d"),
            ("c", "e"),
            ("A", "d"),
            ("A", "e"),
            ("c", "f"),
            ("c", "g"),
            ("f", "B"),
            ("g", "B"),
        };
        var expected8 = new[] { "A-d", "A-e","B-f","B-g"};
        allPassed &= RunTest(edges8, expected8, "Тест 8");
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