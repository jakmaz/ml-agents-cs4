# Training Soccer Agent Features

This guide outlines how to train the soccer agent for each feature. Each feature is tested individually. Please follow the instructions carefully.

## Steps to Train the Model

1. **Navigate to the Experiment Directory**:
   Go to the `Experiments` directory where the configuration files and training scripts are located.

   ```bash
   cd /path/to/ml-agents-cs4/Experiments
   ```

2. **Training Command**:
   Use the following command to start the training process. This will use the `config.yaml` file located in the `Experiments` directory.

   ```bash
   mlagents-learn config.yaml --run-id=<experiment_name>
   ```

   Replace `<experiment_name>` with a unique name for the experiment.

   Example:

   ```bash
   mlagents-learn config.yaml --run-id=avoid-fouls-experiment
   ```

3. **Start Training**:
   Once the training command is run, the model will begin training according to the configurations set in `config.yaml`.

4. **Monitor the Training Process**:
   You can monitor the progress through the Unity editor or by inspecting the logs generated during training. If you need to stop the training, use `Ctrl+C` in the terminal.

5. **Move the Experiment Results**:
   After the training is complete, move the newly created directory (found in `ml-agents-cs4/result/<experiment_name>`) to the `Experiments` directory for tracking.

   ```bash
   mv /path/to/ml-agents-cs4/result/<experiment_name> /path/to/Experiments
   ```

---

## Naming Conventions for Experiments

To ensure consistency, please follow these naming conventions for your experiment directories:

- **Feature Name + Experiment**: Use a clear and descriptive name based on the feature being tested.
  - Example: `avoid-fouls-experiment`, `no-back-rays-experiment`, etc.

---

## List of Features and Experiment Checkboxes

Please check off each experiment after it has been completed.

- [ ] **Avoid Fouls**
- [ ] **No Back Rays**
- [ ] **Decoupled Vision**
- [ ] **Sound Sensor**

---

### Additional Notes

- Each feature should be trained with the same environment settings to ensure consistency in results.
- After training each experiment, evaluate the performance and compare it to the baseline model.
