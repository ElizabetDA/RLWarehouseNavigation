using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class CarrierRobotAgent : Agent
{
    [Header("References")]
    public GridManager grid;
    
    [Header("Position Settings")]
    [SerializeField] private Vector2Int agentPosition;
    [SerializeField] private Vector2Int targetPosition;

    public override void Initialize()
    {
        base.Initialize();
        var actionSpec = ActionSpec.BoundedDiscrete(1, 0, 4);
        SetActionSpec(actionSpec);
        
        agentPosition = Vector2Int.zero;
        targetPosition = Vector2Int.zero;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Наблюдение за полем (2D массив)
        sensor.AddObservation(grid.GetGridAsArray());
        
        // Наблюдение за позицией агента (2 int)
        sensor.AddObservation((float)agentPosition.x);
        sensor.AddObservation((float)agentPosition.y);
        
        // Наблюдение за позицией цели (2 int)
        sensor.AddObservation((float)targetPosition.x);
        sensor.AddObservation((float)targetPosition.y);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        int action = actions.DiscreteActions[0];
        
        // Вычисляем новую позицию
        Vector2Int newPosition = CalculateNewPosition(action);
        
        // Проверяем валидность позиции
        if(grid.IsPositionValid(newPosition))
        {
            agentPosition = newPosition;
            grid.UpdateAgentPosition(agentPosition);
        }
        
        // Расчет награды
        float reward = CalculateReward();
        SetReward(reward);
        
        // Проверка завершения эпизода
        if(agentPosition == targetPosition)
        {
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = 0;
        
        if(Input.GetKey(KeyCode.UpArrow)) discreteActions[0] = 3;
        if(Input.GetKey(KeyCode.DownArrow)) discreteActions[0] = 1;
        if(Input.GetKey(KeyCode.LeftArrow)) discreteActions[0] = 4;
        if(Input.GetKey(KeyCode.RightArrow)) discreteActions[0] = 2;
    }

    private Vector2Int CalculateNewPosition(int action)
    {
        return action switch
        {
            1 => agentPosition + Vector2Int.down,    // Вниз (y-1)
            2 => agentPosition + Vector2Int.right,   // Вправо (x+1)
            3 => agentPosition + Vector2Int.up,      // Вверх (y+1)
            4 => agentPosition + Vector2Int.left,    // Влево (x-1)
            _ => agentPosition                       // Без движения
        };
    }

    public override void OnEpisodeBegin()
    {
        agentPosition = grid.GetRandomEmptyPosition();
        targetPosition = grid.GetRandomTargetPosition();
        grid.UpdatePositions(agentPosition, targetPosition);
    }

    private float CalculateReward()
    {
        // Манхэттенское расстояние для сетки
        int distance = Mathf.Abs(agentPosition.x - targetPosition.x) 
                     + Mathf.Abs(agentPosition.y - targetPosition.y);
        return 1f / (distance + 1f);
    }
}