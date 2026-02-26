import { render, screen } from '@testing-library/react';
import { WizardProgress } from './WizardProgress';

const labels = ['Cuisines', 'Dish Types', 'Diets', 'Intolerances', 'Ingredients'];

describe('WizardProgress', () => {
  it('renders the correct step number in accessible label', () => {
    render(<WizardProgress currentStep={2} totalSteps={5} labels={labels} />);
    expect(screen.getByLabelText('Step 2 of 5')).toBeInTheDocument();
  });

  it('shows the progress bar with correct aria attributes', () => {
    render(<WizardProgress currentStep={3} totalSteps={5} labels={labels} />);
    const progressbar = screen.getByRole('progressbar');
    expect(progressbar).toHaveAttribute('aria-valuenow', '3');
    expect(progressbar).toHaveAttribute('aria-valuemin', '1');
    expect(progressbar).toHaveAttribute('aria-valuemax', '5');
  });

  it('marks the current step with aria-current="step"', () => {
    render(<WizardProgress currentStep={2} totalSteps={5} labels={labels} />);
    const stepIndicators = screen.getAllByText(/\d+|✓/);
    // The element for step 2 (index 1) should have aria-current="step"
    const currentStepEl = stepIndicators.find(
      (el) => el.textContent === '2' && el.getAttribute('aria-current') === 'step'
    );
    expect(currentStepEl).toBeDefined();
  });

  it('renders all step labels', () => {
    render(<WizardProgress currentStep={1} totalSteps={5} labels={labels} />);
    labels.forEach((label) => {
      expect(screen.getByText(label)).toBeInTheDocument();
    });
  });
});
