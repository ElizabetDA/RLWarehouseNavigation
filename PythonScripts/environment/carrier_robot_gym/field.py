import random
from collections import deque


def is_connected(grid, target=0):
    """Проверяет связность всех ячеек с заданным значением (0 или 1)."""
    rows = len(grid)
    cols = len(grid[0]) if rows > 0 else 0
    if rows == 0 or cols == 0:
        return True

    start = None
    for r in range(rows):
        for c in range(cols):
            if grid[r][c] == target:
                start = (r, c)
                break
        if start:
            break
    if not start:
        return True
    visited = set()
    queue = deque([start])
    visited.add(start)
    directions = [(-1, 0), (1, 0), (0, -1), (0, 1)]

    while queue:
        r, c = queue.popleft()
        for dr, dc in directions:
            nr, nc = r + dr, c + dc
            if 0 <= nr < rows and 0 <= nc < cols:
                if grid[nr][nc] == target and (nr, nc) not in visited:
                    visited.add((nr, nc))
                    queue.append((nr, nc))

    for r in range(rows):
        for c in range(cols):
            if grid[r][c] == target and (r, c) not in visited:
                return False
    return True


def has_wall_access(grid):
    """Проверяет, что каждая 1 имеет соседа-0."""
    rows = len(grid)
    cols = len(grid[0]) if rows > 0 else 0
    directions = [(-1, 0), (1, 0), (0, -1), (0, 1)]

    for r in range(rows):
        for c in range(cols):
            if grid[r][c] == 1:
                accessible = False
                for dr, dc in directions:
                    nr, nc = r + dr, c + dc
                    if 0 <= nr < rows and 0 <= nc < cols and grid[nr][nc] == 0:
                        accessible = True
                        break
                if not accessible:
                    return False
    return True


def generate_field(rows, cols, wall_density, max_attempts=10000):
    """Генерирует поле без рекурсии."""
    for _ in range(max_attempts):
        grid = [[1 if random.random() < wall_density else 0 for _ in range(cols)] for _ in range(rows)]

        for r in range(rows):
            for c in range(cols):
                if grid[r][c] == 1:
                    has_access = False
                    for dr, dc in [(-1, 0), (1, 0), (0, -1), (0, 1)]:
                        nr, nc = r + dr, c + dc
                        if 0 <= nr < rows and 0 <= nc < cols and grid[nr][nc] == 0:
                            has_access = True
                            break
                    if not has_access:
                        grid[r][c] = 0

        # Проверяем связность нулей и доступность стен
        if is_connected(grid, target=0) and has_wall_access(grid):
            current_ones = sum(row.count(1) for row in grid)
            target_ones = int(rows * cols * wall_density)
            attempts = 0
            while current_ones < target_ones and attempts < 1000:
                r, c = random.randint(0, rows - 1), random.randint(0, cols - 1)
                if grid[r][c] == 0:
                    grid[r][c] = 1
                    if has_wall_access(grid) and is_connected(grid, target=0):
                        current_ones += 1
                    else:
                        grid[r][c] = 0
                attempts += 1
            return grid

    raise RuntimeError(f"Не удалось сгенерировать поле за {max_attempts} попыток")


if __name__ == "__main__":
    rows, cols = 5, 5
    wall_density = 0.5  # 30% стен
    field = generate_field(rows, cols, wall_density)

    for row in field:
        print(" ".join(map(str, row)))