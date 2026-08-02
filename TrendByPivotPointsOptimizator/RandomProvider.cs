using System;

namespace TrendByPivotPointsOptimizator
{
    public class RandomProvider : IRandomProvider
    {
        private Random random;

        /// <summary>Сид, которым генератор был создан.</summary>
        public int Seed { get; }

        /// <summary>
        /// Сколько чисел уже выдано. Вместе с сидом полностью описывает состояние
        /// генератора: чтобы продолжить прогон с чек-поинта, достаточно создать
        /// генератор с тем же сидом и прокрутить столько же обращений.
        /// </summary>
        public int DrawsCount { get; private set; }

        public RandomProvider() : this(Environment.TickCount) { }

        public RandomProvider(int seed)
        {
            Seed = seed;
            random = new Random(seed);
        }

        public double NextDouble()
        {
            DrawsCount++;
            return random.NextDouble();
        }

        public int Next(int maxValue)
        {
            DrawsCount++;
            return random.Next(maxValue);
        }

        public int Next(int minValue, int maxValue)
        {
            DrawsCount++;
            return random.Next(minValue, maxValue);
        }

        /// <summary>
        /// Прокручивает генератор до нужного количества обращений — так прогон,
        /// продолженный с чек-поинта, идёт по той же последовательности случайных
        /// чисел, что и непрерывный. Каждое обращение продвигает состояние Random
        /// ровно на один шаг, поэтому холостых вызовов достаточно.
        /// </summary>
        public void Replay(int drawsCount)
        {
            if (drawsCount < DrawsCount)
                throw new ArgumentException(
                    $"Генератор уже выдал {DrawsCount} чисел, прокрутить его назад " +
                    $"до {drawsCount} нельзя.", nameof(drawsCount));

            while (DrawsCount < drawsCount)
                NextDouble();
        }
    }
}
