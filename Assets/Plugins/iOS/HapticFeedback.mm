#import <UIKit/UIKit.h>

extern "C"
{
    void _TriggerImpactLight()
    {
        UIImpactFeedbackGenerator *gen =
            [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleLight];
        [gen prepare];
        [gen impactOccurred];
    }

    void _TriggerImpactMedium()
    {
        UIImpactFeedbackGenerator *gen =
            [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleMedium];
        [gen prepare];
        [gen impactOccurred];
    }

    void _TriggerImpactHeavy()
    {
        UIImpactFeedbackGenerator *gen =
            [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleHeavy];
        [gen prepare];
        [gen impactOccurred];
    }
}
