from time import sleep
import gymnasium as gym
import gymnasium.spaces
import numpy as np
from gymnasium import register
from matplotlib import pyplot as plt
from stable_baselines3 import PPO, A2C
from stable_baselines3.common.callbacks import BaseCallback
from stable_baselines3.common.env_util import make_vec_env
from stable_baselines3.common.vec_env import SubprocVecEnv

from environment.carrier_robot_gym.carrier_robot_gym import CarrierRobotEnv, CELL_EMPTY, CELL_BUSY
from collections import defaultdict


class RewardLoggerCallback(BaseCallback):
    def __init__(self, verbose=0):
        super().__init__(verbose)
        self.episode_rewards = []
        self.episode_lengths = []
        self.step_rewards = []
        self.episode_info = defaultdict(list)
        self.current_episode_rewards = 0.0
        self.current_episode_length = 0
        self.average_rewards = []
        self.average_reward = 0
        self.count = 0

    def _on_step(self) -> bool:
        infos = self.locals.get('infos', [{}])
        dones = self.locals.get('dones', [False])
        rewards = self.locals.get('rewards', [0.0])
        self.count += 1
        if self.count % 100_000 == 0:
            self.step_rewards.append(rewards[0])
            self.current_episode_rewards += rewards[0]
            self.current_episode_length += 1

            self.average_reward += rewards[0]

            self.average_rewards.append(self.average_reward / self.count)

            if dones[0]:
                self.episode_rewards.append(self.current_episode_rewards)
                self.episode_lengths.append(self.current_episode_length)

                # Сохраняем только числовые метрики из инфо
                for key, value in infos[0].items():
                    if isinstance(value, (int, float, np.number)):
                        self.episode_info[key].append(float(value))

                self.current_episode_rewards = 0.0
                self.current_episode_length = 0

        return True

    def _on_rollout_end(self):
        if len(self.episode_rewards) > 0:
            print("\n--- Episode Stats ---")
            print(f"Mean Reward: {np.mean(self.episode_rewards):.2f}")
            print(f"Mean Length: {np.mean(self.episode_lengths):.2f}")

            # Выводим только числовые метрики
            for key in self.episode_info:
                if len(self.episode_info[key]) > 0:
                    print(f"{key}: {np.mean(self.episode_info[key]):.2f}")
            print("----------------------\n")


def learn(width: int = 10, height: int = 10):
    # env = CarrierRobotEnv(width=width, height=height, render_mode='human')
    callback = RewardLoggerCallback()

    env = make_vec_env(
        env_id='CarrierRobot-v0',
        n_envs=4,
        env_kwargs={"width": width, "height": height, "render_mode": "human"}

    )
    #
    # # Инициализация PPO
    # model = PPO(
    #     "MultiInputPolicy",
    #     env=env,
    #     learning_rate=3e-4,
    #     n_steps=2048,
    #     batch_size=64,
    #     gamma=0.99,
    #     verbose=1,
    #     device="auto"
    # )
    model = PPO.load('models/CarrierRobot v33 10x10 20m.zip', env=env)

    # model = PPO.load('models/CarrierRobot v25 5x5 10m', env=env)
    model.learn(total_timesteps=20_000, callback=callback)
    model.save('CarrierRobot')

    # Визуализация данных
    plt.figure(figsize=(14, 8))

    # График наград за шаг
    plt.subplot(3, 1, 1)
    plt.plot(callback.step_rewards, alpha=0.3)

    plt.ylim(-20)
    plt.title('Step Rewards')
    plt.xlabel('Step')

    # Гистограмма наград
    plt.subplot(3, 1, 2)
    plt.hist(callback.step_rewards, bins=50)
    plt.title('Rewards Distribution')

    # График наград за эпизод
    plt.subplot(3, 1, 3)
    plt.plot(callback.average_rewards)
    plt.ylim(-15, 10)
    plt.title('Average Rewards')
    plt.grid()
    plt.xlabel('Episode')

    plt.tight_layout()
    plt.savefig('training_stats.png')
    plt.show()


def play(version: str, width: int = 10, height: int = 10, count_agents: int = 3):
    envs = []
    for i in range(count_agents):
        envs.append(CarrierRobotEnv(width=width, height=height, render_mode='human'))

    model = PPO.load(version)
    envs = reset(envs, count_agents)
    field = envs[0].field

    while True:
        for i in range(count_agents):
            envs[i].field = field
            action, state = model.predict(envs[i].get_obs())
            print(i + 1, '---', action)
            obs, reward, terminated, _, info = envs[i].step(action)

            field = obs['field']
            render(envs, field, width, height)


            if terminated and 'win' in info:
                target_pos = envs[i].find_random_cell(CELL_BUSY)
                while any(np.array_equal(envs[j].pos, target_pos) for j in range(count_agents)):
                    target_pos = envs[i].find_random_cell(CELL_BUSY)

                envs[i].target = target_pos

            elif terminated:
                envs = reset(envs, count_agents)
                field = envs[0].field
                break


        for i in range(count_agents):
            envs[i].field = field


def reset(envs, count_agents):
    envs[0].reset()

    # Поле спец
    # envs[0].field = np.array([
    #     [0, 1, 1, 1, 0],
    #     [1, 1, 0, 0, 1],
    #     [1, 0, 0, 0, 1],
    #     [1, 0, 0, 0, 1],
    #     [0, 1, 1, 1, 0],
    # ])
    # envs[0].pos = np.array([1, 1])
    # envs[0].target = np.array([2, 4])
    # end

    field: np.ndarray[np.int8] = envs[0].field
    for i in range(1, count_agents):
        envs[i].field = field
        envs[i].pos = envs[i].find_random_cell(CELL_EMPTY)
        field[tuple(envs[i].pos)] = CELL_BUSY

        target_pos = envs[i].find_random_cell(CELL_BUSY)
        while any(np.array_equal(envs[j].pos, target_pos) for j in range(count_agents)):
            target_pos = envs[i].find_random_cell(CELL_BUSY)

        envs[i].target = target_pos

        field = envs[i].field

    for i in range(count_agents):
        envs[i].field = field

    return envs


def render(envs, field, width: int = 10, height: int = 10) -> None:
    plt.clf()
    # Отрисовка сетки
    plt.grid(True, color='gray', linestyle='--', linewidth=0.5)
    plt.xticks(np.arange(0, width + 1, 1))
    plt.yticks(np.arange(0, height + 1, 1))
    for i in range(height):
        for j in range(width):
            if field[i][j] == 1:
                plt.scatter(j + 0.5, i + 0.5, s=500, c='gray', marker='s')

    for i in range(len(envs)):
        plt.scatter(envs[i].target[1] + 0.5, envs[i].target[0] + 0.5, s=500, c='green', marker='s')
        # print(i, envs[i].target)
        plt.scatter(envs[i].pos[1] + 0.5, envs[i].pos[0] + 0.5, s=500, c='blue', marker='o')
    plt.xlim(0, width)
    plt.ylim(height, 0)
    plt.gca().set_aspect('equal')
    plt.pause(0.01)


if __name__ == '__main__':
    play(version="models/CarrierRobot v58 5x5 180m.zip", width=5, height=5, count_agents=1)
    # learn(10, 10)
