from mlagents_envs.environment import UnityEnvironment
from mlagents_envs.base_env import ActionTuple
import numpy as np
from gymnasium import spaces
import gymnasium as gym


class UnityCarrierWrapper(gym.Env):
    """
    Обертка для Unity-среды, совместимая с Gymnasium API
    Преобразует коммуникацию с Unity в формат, понятный Stable Baselines3
    """

    def __init__(self, unity_build_path: str):
        super().__init__()

        # 1. Инициализация Unity-среды
        self.unity_env = UnityEnvironment(
            file_name=unity_build_path,
            seed=42,
            side_channels=[],
            no_graphics=True  # Ускорение за счет отключения рендеринга
        )
        self.unity_env.reset()

        # 2. Получение спецификаций поведения агента
        self.behavior_name = list(self.unity_env.behavior_specs.keys())[0]
        self.spec = self.unity_env.behavior_specs[self.behavior_name]

        # 3. Определение пространств действий и наблюдений
        self._define_spaces()

        # 4. Инициализация внутреннего состояния
        self.current_obs = None

    def _define_spaces(self):
        """Создание Gym-совместимых пространств на основе данных из Unity"""

        # Парсинг наблюдений из Unity
        obs_shapes = [obs_spec.shape for obs_spec in self.spec.observation_specs]

        # Предполагаем, что Unity отправляет данные в порядке:
        # [поле, позиция агента, позиция цели]
        self.observation_space = spaces.Dict({
            'field': spaces.Box(
                low=0,
                high=2,
                shape=obs_shapes[0],
                dtype=np.int8
            ),
            'pos': spaces.Box(
                low=0,
                high=np.inf,
                shape=obs_shapes[1],
                dtype=np.int64
            ),
            'target': spaces.Box(
                low=0,
                high=np.inf,
                shape=obs_shapes[2],
                dtype=np.int64
            )
        })

        # Для дискретных действий (5 вариантов: 0-4)
        self.action_space = spaces.Discrete(5)

    def _process_unity_obs(self, obs_list):
        """Преобразование сырых наблюдений из Unity в Gym-формат"""
        return {
            'field': obs_list[0].astype(np.int8),
            'pos': obs_list[1].flatten().astype(np.int64),
            'target': obs_list[2].flatten().astype(np.int64)
        }

    def reset(self, **kwargs):
        """Сброс среды и получение начальных наблюдений"""
        self.unity_env.reset()
        decision_steps, _ = self.unity_env.get_steps(self.behavior_name)
        self.current_obs = self._process_unity_obs(decision_steps.obs)
        return self.current_obs, {}

    def step(self, action: int):
        """Выполнение одного шага в среде"""
        # 1. Создание ActionTuple для Unity
        action_tuple = ActionTuple()
        action_tuple.add_discrete(np.array([[action]], dtype=np.int32))

        # 2. Отправка действия в Unity
        self.unity_env.set_actions(self.behavior_name, action_tuple)
        self.unity_env.step()

        # 3. Получение новых наблюдений
        decision_steps, terminal_steps = self.unity_env.get_steps(self.behavior_name)
        done = len(terminal_steps) > 0

        # 4. Обработка завершения эпизода
        if done:
            reward = terminal_steps.reward[0]
            new_obs = self._process_unity_obs(terminal_steps.obs)
        else:
            reward = decision_steps.reward[0]
            new_obs = self._process_unity_obs(decision_steps.obs)

        self.current_obs = new_obs

        # 5. Возвращаем данные в формате Gym
        return new_obs, float(reward), done, False, {}

    def close(self):
        """Корректное завершение среды"""
        self.unity_env.close()

    def render(self, mode='human'):
        """Режим рендеринга (опционально)"""
        if mode == 'rgb_array':
            return self.current_obs['field']
        return None