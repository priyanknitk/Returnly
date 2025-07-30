import React from 'react';
import TaxResults from './TaxResults';
import TaxCalculationError from './TaxCalculationError';
import { TaxResultsResponse, isTaxResultsError } from '../utils/taxResultsMapper';

interface TaxResultsWrapperProps {
  results: TaxResultsResponse;
  onRetry?: () => void;
  onBack?: () => void;
  onGenerateITR?: () => void;
  showITRButton?: boolean;
}

const TaxResultsWrapper: React.FC<TaxResultsWrapperProps> = ({
  results,
  onRetry,
  onBack,
  onGenerateITR,
  showITRButton = false
}) => {
  // Check if results contain an error
  if (isTaxResultsError(results)) {
    return (
      <TaxCalculationError
        error={results}
        onRetry={onRetry}
        onBack={onBack}
      />
    );
  }

  // Render successful tax results
  return (
    <TaxResults
      taxCalculation={results.taxCalculation}
      refundCalculation={results.refundCalculation}
      onGenerateITR={onGenerateITR}
      showITRButton={showITRButton}
    />
  );
};

export default TaxResultsWrapper;
