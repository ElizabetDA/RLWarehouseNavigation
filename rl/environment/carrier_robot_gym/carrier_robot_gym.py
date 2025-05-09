import random
from typing import Optional, Any, SupportsFloat, Dict

import gymnasium
import numpy as np
from gymnasium.core import ActType, ObsType
from matplotlib import pyplot as plt

from environment.carrier_robot_gym.field import generate_field

CELL_EMPTY = 0
CELL_BUSY = 1


class CarrierRobotEnv(gymnasium.Env):
    metadata = {'render_modes': ['human', 'rgb_array'], 'render_fps': 4}

    def __init__(self, width: int = 10, height: int = 10, render_mode=None):
        super().__init__()
        if width <= 1 or height <= 1:
            raise ValueError('Height and width must be greater than 1')

        self.width = width
        self.height = height
        self.render_mode = render_mode
        self.time = 0

        self.observation_space = gymnasium.spaces.Dict({
            'field': gymnasium.spaces.Box(low=0, high=2, shape=(height, width), dtype=np.int8),
            'pos': gymnasium.spaces.Box(low=0, high=max(width, height) - 1, shape=(2,), dtype=np.int64),
            'target': gymnasium.spaces.Box(low=0, high=max(width, height) - 1, shape=(2,), dtype=np.int64),
        })
        self.action_space = gymnasium.spaces.Discrete(5)

        self.field = np.zeros((height, width), dtype=np.int8)
        self.pos = np.array([0, 0])
        self.target = np.array([0, 0])
        self.wall_density = 0.3

    def reset(self, seed: Optional[int] = None, options: Optional[dict] = None) -> tuple[Dict, Any]:
        super().reset(seed=seed)
        self.wall_density = random.uniform(0.2, 0.3)

        # self.field = self.generate_new_field(self.width, self.height)
        self.field = np.array(generate_field(self.height, self.width, self.wall_density, 10000))


        self.target = self.find_random_cell(CELL_BUSY)


        self.pos = self.find_random_cell(CELL_EMPTY)
        self.field[tuple(self.pos)] = CELL_BUSY

        # start TODO
        # Поиск максимально отдаленной ячейки(для дообучения)
        # self.target = self.find_the_most_remote_cell(self.pos, CELL_BUSY)

        # end

        self.time = 0
        return self.get_obs(), {}

    def step(self, action: ActType) -> tuple[Dict, SupportsFloat, bool, bool, Dict]:
        direction = self.calculate_new_position(action)
        reward = 0
        terminated = False
        info = {}


        if self._is_adjacent(self.pos, self.target):
            reward += 1
            terminated = True
            info = {'win': True}
            return self.get_obs(), reward, terminated, False, info

        if not self._is_valid_position(direction):
            reward = -11
            terminated = True
        elif self.field[tuple(direction)] == CELL_BUSY and not np.array_equal(self.pos, direction):
            reward = -1
            terminated = True
        else:
            self.field[tuple(self.pos)] = CELL_EMPTY
            self.pos = direction
            self.field[tuple(self.pos)] = CELL_BUSY

        if self._is_adjacent(self.pos, self.target):
            reward += 1
            terminated = True
            info = {'win': True}


        reward -= 1.25 / (self.time + 1)
        self.time += 1

        return self.get_obs(), reward, terminated, False, info

    def render(self) -> Optional[np.ndarray]:
        if self.render_mode == 'human':
            plt.clf()
            plt.imshow(self.field, cmap='viridis', origin='lower')
            plt.scatter(*self.pos[::-1], c='red', s=200, marker='s')
            plt.scatter(*self.target[::-1], c='green', s=200, marker='s')
            plt.title(f'Time: {self.time}')
            plt.pause(0.1 / self.metadata['render_fps'])
        elif self.render_mode == 'rgb_array':
            return self.field.copy()

    def find_random_cell(self, cell_type: int) -> np.ndarray:
        positions = np.argwhere(self.field == cell_type)
        return positions[np.random.choice(len(positions))]

    def _is_valid_position(self, pos: np.ndarray) -> np.ndarray[tuple[int, ...], np.dtype[bool]]:
        return (0 <= pos[0] < self.height) and (0 <= pos[1] < self.width)

    @staticmethod
    def _is_adjacent(pos1: np.ndarray, pos2: np.ndarray) -> bool:
        return abs(pos1[0] - pos2[0]) + abs(pos1[1] - pos2[1]) == 1

    def calculate_new_position(self, action: int = 0) -> np.ndarray:
        return np.clip(self.pos + [
            (0, 0),  # Бездействие
            (1, 0),  # Вниз
            (0, 1),  # Вправо
            (-1, 0),  # Вверх
            (0, -1)  # Влево
        ][action], 0, [self.height - 1, self.width - 1])

    def generate_new_field(self, width, height) -> np.ndarray:
        wall_density = random.uniform(0.3, 0.5)
        field = np.zeros((height, width))
        for i in range(int(height * width * wall_density)):
            pos = self.find_random_cell(CELL_EMPTY)
            field[tuple(pos)] = CELL_BUSY
        return field

    def find_the_most_remote_cell(self, center: np.ndarray[tuple[int, ...], np.dtype] ,cell_type: int):
        cell = center
        for i in range(self.height):
            for j in range(self.width):
                if self.field[i][j] == cell_type and \
                    abs(i - center[0]) ** 2 + abs(j - center[1]) ** 2 > abs(cell[0] - center[0]) ** 2 + abs(cell[1] - center[1]) ** 2:
                    cell = [i, j]
        return cell

    def get_obs(self) -> Dict:
        return {
            'field': self.field.copy(),
            'pos': self.pos.copy(),
            'target': self.target.copy()
        }