#pragma once
#ifndef GOSF_AUTOMATION_RESULT_H_
#define GOSF_AUTOMATION_RESULT_H_

namespace gosf::internal {

template <typename Result, typename Value>
void set_result(Result *result, Value value) noexcept {
    if (result)
        *result = static_cast<Result>(value);
}

} // namespace gosf::internal

#endif // GOSF_AUTOMATION_RESULT_H_
