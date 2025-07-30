// components/requests/RequestFormContainer.tsx
import { useState } from 'react';
import RequestForm from '../RequestForm/RequestForm';
import VacationIllustration from './VacationIllustration';
import SickIllustration from './SickIllustration';
import OtherIllustration from './OtherIllustration';


interface RequestFormContainerProps {
    onSubmit: (values: {
        startDate: string;
        endDate: string;
        type: 'Vacation' | 'Sick' | 'Other';
        reason?: string;
    }) => void;
}

export default function RequestFormContainer({ onSubmit }: RequestFormContainerProps) {
    const [showVacationIllustration, setShowVacationIllustration] = useState(false);
    const [showSickIllustration, setShowSickIllustration] = useState(false);
    const [showOtherIllustration, setShowOtherIllustration] = useState(false);

    return (
        <div className="relative">
            {showVacationIllustration && (
                <div
                    key="vacation" 
                    className="fixed right-0 bottom-0 z-0 w-[300px] h-[280px] animate-drop-bounce"
                >
                    <VacationIllustration />
                </div>
            )}


            {showSickIllustration && (
                <div className="fixed left-0 bottom-0 z-0 w-[300px] h-[280px] animate-drop-bounce">
                    <SickIllustration />
                </div>
            )}

            {showOtherIllustration && (
                <div className="fixed left-0 bottom-0 z-0 w-[300px] h-[280px] animate-drop-bounce">
                    <OtherIllustration />
                </div>
            )}

            <div className="relative z-10">
                <RequestForm
                    onSubmit={onSubmit}
                    onTypeChange={(type) => {
                        setShowVacationIllustration(type === 'Vacation');
                        setShowSickIllustration(type === 'Sick');
                        setShowOtherIllustration(type === 'Other');
                    }}
                />

            </div>
        </div>
    );
}