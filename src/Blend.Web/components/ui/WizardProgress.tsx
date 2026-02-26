'use client';

interface WizardProgressProps {
  currentStep: number;
  totalSteps: number;
  labels: string[];
}

export function WizardProgress({ currentStep, totalSteps, labels }: WizardProgressProps) {
  return (
    <div className="w-full" aria-label={`Step ${currentStep} of ${totalSteps}`}>
      <div className="flex items-center justify-between mb-2">
        {labels.map((label, index) => (
          <div key={label} className="flex flex-col items-center flex-1">
            <div
              className={`w-8 h-8 rounded-full flex items-center justify-center text-sm font-semibold ${
                index + 1 < currentStep
                  ? 'bg-primary text-white'
                  : index + 1 === currentStep
                  ? 'bg-primary text-white ring-2 ring-offset-2 ring-primary'
                  : 'bg-gray-200 text-gray-500'
              }`}
              aria-current={index + 1 === currentStep ? 'step' : undefined}
            >
              {index + 1 < currentStep ? '✓' : index + 1}
            </div>
            <span className="text-xs mt-1 text-center text-gray-600 hidden sm:block">{label}</span>
          </div>
        ))}
      </div>
      <div className="w-full bg-gray-200 rounded-full h-1.5">
        <div
          className="bg-primary h-1.5 rounded-full transition-all duration-300"
          style={{ width: `${((currentStep - 1) / (totalSteps - 1)) * 100}%` }}
          role="progressbar"
          aria-valuenow={currentStep}
          aria-valuemin={1}
          aria-valuemax={totalSteps}
        />
      </div>
    </div>
  );
}
