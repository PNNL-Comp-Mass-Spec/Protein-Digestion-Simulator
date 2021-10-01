using System;

namespace ProteinDigestionSimulator
{
    // -------------------------------------------------------------------------------
    // Written by Matthew Monroe for the Department of Energy (PNNL, Richland, WA) in 2003
    //
    // E-mail: matthew.monroe@pnnl.gov or proteomics@pnnl.gov
    // Website: https://omics.pnl.gov/ or https://www.pnnl.gov/sysbio/ or https://panomics.pnnl.gov/
    // -------------------------------------------------------------------------------
    //
    // Licensed under the 2-Clause BSD License; you may not use this file except
    // in compliance with the License.  You may obtain a copy of the License at
    // https://opensource.org/licenses/BSD-2-Clause
    //
    // Copyright 2018 Battelle Memorial Institute

    /// <summary>
    /// This class can be used to search a list of values for a given value, plus or minus a given tolerance
    /// The input list need not be sorted, since mPointerIndices[] will be populated when the data is loaded,
    /// after which the data array will be sorted
    ///
    /// To prevent this behavior, and save memory by not populating mPointerIndices, set mUsePointerIndexArray = False
    /// </summary>
    public class SearchRange
    {
        public SearchRange()
        {
            InitializeLocalVariables();
        }

        private enum DataTypeToUse
        {
            NoDataPresent = 0,
            IntegerType = 1,
            SingleType = 2,
            DoubleType = 3,
            FillingIntegerType = 4,
            FillingSingleType = 5,
            FillingDoubleType = 6
        }

        private DataTypeToUse mDataType;

        private int[] mDataInt;
        private float[] mDataSingle;
        private double[] mDataDouble;

        private int mPointByPointFillCount;

        private int[] mPointerIndices;        // Pointers to the original index of the data point in the source array

        private bool mPointerArrayIsValid;

        public int DataCount
        {
            get
            {
                switch (mDataType)
                {
                    case DataTypeToUse.IntegerType:
                    case DataTypeToUse.FillingIntegerType:
                        return mDataInt.Length;
                    case DataTypeToUse.SingleType:
                    case DataTypeToUse.FillingSingleType:
                        return mDataSingle.Length;
                    case DataTypeToUse.DoubleType:
                    case DataTypeToUse.FillingDoubleType:
                        return mDataDouble.Length;
                    case DataTypeToUse.NoDataPresent:
                        return 0;
                    default:
                        Console.WriteLine("Unknown data type encountered: " + mDataType);
                        return 0;
                }
            }
        }

        public int get_OriginalIndex(int index)
        {
            if (mPointerArrayIsValid)
            {
                try
                {
                    if (index < mPointerIndices.Length)
                    {
                        return mPointerIndices[index];
                    }

                    return -1;
                }
                catch
                {
                    return -1;
                }
            }

            return -1;
        }

        // ReSharper disable once UnusedMember.Global
        public bool UsePointerIndexArray { get; set; }

        private void BinarySearchRangeInt(int searchValue, int toleranceHalfWidth, ref int matchIndexStart, ref int matchIndexEnd)
        {
            // Recursive search function

            var leftDone = default(bool);
            var rightDone = default(bool);

            var indexMidpoint = (matchIndexStart + matchIndexEnd) / 2;
            if (indexMidpoint == matchIndexStart)
            {
                // Min and Max are next to each other
                if (Math.Abs(searchValue - mDataInt[matchIndexStart]) > toleranceHalfWidth)
                {
                    matchIndexStart = matchIndexEnd;
                }

                if (Math.Abs(searchValue - mDataInt[matchIndexEnd]) > toleranceHalfWidth)
                {
                    matchIndexEnd = indexMidpoint;
                }

                return;
            }

            if (mDataInt[indexMidpoint] > searchValue + toleranceHalfWidth)
            {
                // Out of range on the right
                matchIndexEnd = indexMidpoint;
                BinarySearchRangeInt(searchValue, toleranceHalfWidth, ref matchIndexStart, ref matchIndexEnd);
            }
            else if (mDataInt[indexMidpoint] < searchValue - toleranceHalfWidth)
            {
                // Out of range on the left
                matchIndexStart = indexMidpoint;
                BinarySearchRangeInt(searchValue, toleranceHalfWidth, ref matchIndexStart, ref matchIndexEnd);
            }
            else
            {
                // Inside range; figure out the borders
                var leftIndex = indexMidpoint;
                while (!leftDone)
                {
                    leftIndex -= 1;
                    if (leftIndex < matchIndexStart)
                    {
                        leftDone = true;
                    }
                    else if (Math.Abs(searchValue - mDataInt[leftIndex]) > toleranceHalfWidth)
                    {
                        leftDone = true;
                    }
                }

                var rightIndex = indexMidpoint;

                while (!rightDone)
                {
                    rightIndex += 1;
                    if (rightIndex > matchIndexEnd)
                    {
                        rightDone = true;
                    }
                    else if (Math.Abs(searchValue - mDataInt[rightIndex]) > toleranceHalfWidth)
                    {
                        rightDone = true;
                    }
                }

                matchIndexStart = leftIndex + 1;
                matchIndexEnd = rightIndex - 1;
            }
        }

        private void BinarySearchRangeSng(float searchValue, float toleranceHalfWidth, ref int matchIndexStart, ref int matchIndexEnd)
        {
            // Recursive search function

            var leftDone = default(bool);
            var rightDone = default(bool);

            var indexMidpoint = (matchIndexStart + matchIndexEnd) / 2;
            if (indexMidpoint == matchIndexStart)
            {
                // Min and Max are next to each other
                if (Math.Abs(searchValue - mDataSingle[matchIndexStart]) > toleranceHalfWidth)
                {
                    matchIndexStart = matchIndexEnd;
                }

                if (Math.Abs(searchValue - mDataSingle[matchIndexEnd]) > toleranceHalfWidth)
                {
                    matchIndexEnd = indexMidpoint;
                }

                return;
            }

            if (mDataSingle[indexMidpoint] > searchValue + toleranceHalfWidth)
            {
                // Out of range on the right
                matchIndexEnd = indexMidpoint;
                BinarySearchRangeSng(searchValue, toleranceHalfWidth, ref matchIndexStart, ref matchIndexEnd);
            }
            else if (mDataSingle[indexMidpoint] < searchValue - toleranceHalfWidth)
            {
                // Out of range on the left
                matchIndexStart = indexMidpoint;
                BinarySearchRangeSng(searchValue, toleranceHalfWidth, ref matchIndexStart, ref matchIndexEnd);
            }
            else
            {
                // Inside range; figure out the borders
                var leftIndex = indexMidpoint;
                while (!leftDone)
                {
                    leftIndex -= 1;
                    if (leftIndex < matchIndexStart)
                    {
                        leftDone = true;
                    }
                    else if (Math.Abs(searchValue - mDataSingle[leftIndex]) > toleranceHalfWidth)
                    {
                        leftDone = true;
                    }
                }

                var rightIndex = indexMidpoint;

                while (!rightDone)
                {
                    rightIndex += 1;
                    if (rightIndex > matchIndexEnd)
                    {
                        rightDone = true;
                    }
                    else if (Math.Abs(searchValue - mDataSingle[rightIndex]) > toleranceHalfWidth)
                    {
                        rightDone = true;
                    }
                }

                matchIndexStart = leftIndex + 1;
                matchIndexEnd = rightIndex - 1;
            }
        }

        private void BinarySearchRangeDbl(double searchValue, double toleranceHalfWidth, ref int matchIndexStart, ref int matchIndexEnd)
        {
            // Recursive search function

            var leftDone = default(bool);
            var rightDone = default(bool);

            var indexMidpoint = (matchIndexStart + matchIndexEnd) / 2;
            if (indexMidpoint == matchIndexStart)
            {
                // Min and Max are next to each other
                if (Math.Abs(searchValue - mDataDouble[matchIndexStart]) > toleranceHalfWidth)
                {
                    matchIndexStart = matchIndexEnd;
                }

                if (Math.Abs(searchValue - mDataDouble[matchIndexEnd]) > toleranceHalfWidth)
                {
                    matchIndexEnd = indexMidpoint;
                }

                return;
            }

            if (mDataDouble[indexMidpoint] > searchValue + toleranceHalfWidth)
            {
                // Out of range on the right
                matchIndexEnd = indexMidpoint;
                BinarySearchRangeDbl(searchValue, toleranceHalfWidth, ref matchIndexStart, ref matchIndexEnd);
            }
            else if (mDataDouble[indexMidpoint] < searchValue - toleranceHalfWidth)
            {
                // Out of range on the left
                matchIndexStart = indexMidpoint;
                BinarySearchRangeDbl(searchValue, toleranceHalfWidth, ref matchIndexStart, ref matchIndexEnd);
            }
            else
            {
                // Inside range; figure out the borders
                var leftIndex = indexMidpoint;
                while (!leftDone)
                {
                    leftIndex -= 1;
                    if (leftIndex < matchIndexStart)
                    {
                        leftDone = true;
                    }
                    else if (Math.Abs(searchValue - mDataDouble[leftIndex]) > toleranceHalfWidth)
                    {
                        leftDone = true;
                    }
                }

                var rightIndex = indexMidpoint;

                while (!rightDone)
                {
                    rightIndex += 1;
                    if (rightIndex > matchIndexEnd)
                    {
                        rightDone = true;
                    }
                    else if (Math.Abs(searchValue - mDataDouble[rightIndex]) > toleranceHalfWidth)
                    {
                        rightDone = true;
                    }
                }

                matchIndexStart = leftIndex + 1;
                matchIndexEnd = rightIndex - 1;
            }
        }

        private void ClearUnusedData()
        {
            if (mDataType != DataTypeToUse.IntegerType)
            {
                mDataInt = new int[0];
            }

            if (mDataType != DataTypeToUse.SingleType)
            {
                mDataSingle = new float[0];
            }

            if (mDataType != DataTypeToUse.DoubleType)
            {
                mDataDouble = new double[0];
            }

            if (mDataType == DataTypeToUse.NoDataPresent)
            {
                mPointerIndices = new int[0];
                mPointerArrayIsValid = false;
            }
        }

        public void ClearData()
        {
            mDataType = DataTypeToUse.NoDataPresent;
            ClearUnusedData();
        }

        public void InitializeDataFillInteger(int expectedDataCount)
        {
            // Call this sub to initialize the data arrays, which will allow you to
            // then call FillWithDataAddPoint() repeatedly for each data point
            // or call FillWithDataAddBlock() repeatedly with each block of data points
            // When done, call FinalizeDataFill

            mDataType = DataTypeToUse.NoDataPresent;
            ClearUnusedData();

            mDataType = DataTypeToUse.FillingIntegerType;
            mDataInt = new int[expectedDataCount];

            mPointByPointFillCount = 0;
        }

        public void InitializeDataFillSingle(int dataCountToReserve)
        {
            // Call this sub to initialize the data arrays, which will allow you to
            // then call FillWithDataAddPoint() repeatedly for each data point
            // or call FillWithDataAddBlock() repeatedly with each block of data points
            // When done, call FinalizeDataFill

            mDataType = DataTypeToUse.NoDataPresent;
            ClearUnusedData();

            mDataType = DataTypeToUse.FillingSingleType;
            mDataSingle = new float[dataCountToReserve];
        }

        public void InitializeDataFillDouble(int dataCountToReserve)
        {
            // Call this sub to initialize the data arrays, which will allow you to
            // then call FillWithDataAddPoint() repeatedly for each data point
            // or call FillWithDataAddBlock() repeatedly with each block of data points
            // When done, call FinalizeDataFill

            mDataType = DataTypeToUse.NoDataPresent;
            ClearUnusedData();

            mDataType = DataTypeToUse.FillingDoubleType;
            mDataDouble = new double[dataCountToReserve];
        }

        public bool FillWithData(ref int[] values)
        {
            bool success;

            try
            {
                if (values == null || values.Length == 0)
                {
                    success = false;
                }
                else
                {
                    mDataInt = new int[values.Length];
                    values.CopyTo(mDataInt, 0);
                    if (UsePointerIndexArray)
                    {
                        InitializePointerIndexArray(mDataInt.Length);
                        Array.Sort(mDataInt, mPointerIndices);
                    }
                    else
                    {
                        Array.Sort(mDataInt);
                        mPointerIndices = new int[0];
                        mPointerArrayIsValid = false;
                    }

                    mDataType = DataTypeToUse.IntegerType;
                    success = true;
                }
            }
            catch
            {
                success = false;
            }

            if (success)
            {
                ClearUnusedData();
            }
            else
            {
                mDataType = DataTypeToUse.NoDataPresent;
            }

            return success;
        }

        public bool FillWithData(ref float[] values)
        {
            bool success;

            try
            {
                if (values == null || values.Length == 0)
                {
                    success = false;
                }
                else
                {
                    mDataSingle = new float[values.Length];
                    values.CopyTo(mDataSingle, 0);
                    if (UsePointerIndexArray)
                    {
                        InitializePointerIndexArray(mDataSingle.Length);
                        Array.Sort(mDataSingle, mPointerIndices);
                    }
                    else
                    {
                        Array.Sort(mDataSingle);
                        mPointerIndices = new int[0];
                        mPointerArrayIsValid = false;
                    }

                    mDataType = DataTypeToUse.SingleType;
                    success = true;
                }
            }
            catch
            {
                success = false;
            }

            if (success)
            {
                ClearUnusedData();
            }
            else
            {
                mDataType = DataTypeToUse.NoDataPresent;
            }

            return success;
        }

        public bool FillWithData(ref double[] values)
        {
            bool success;

            try
            {
                if (values == null || values.Length == 0)
                {
                    success = false;
                }
                else
                {
                    mDataDouble = new double[values.Length];
                    values.CopyTo(mDataDouble, 0);

                    if (UsePointerIndexArray)
                    {
                        InitializePointerIndexArray(mDataDouble.Length);
                        Array.Sort(mDataDouble, mPointerIndices);
                    }
                    else
                    {
                        Array.Sort(mDataDouble);
                        mPointerIndices = new int[0];
                        mPointerArrayIsValid = false;
                    }

                    mDataType = DataTypeToUse.DoubleType;
                    success = true;
                }
            }
            catch
            {
                success = false;
            }

            if (success)
            {
                ClearUnusedData();
            }
            else
            {
                mDataType = DataTypeToUse.NoDataPresent;
            }

            return success;
        }

        public bool FillWithDataAddBlock(int[] valuesToAdd)
        {
            var success = true;

            try
            {
                if (mDataInt.Length <= mPointByPointFillCount + valuesToAdd.Length - 1)
                {
                    Array.Resize(ref mDataInt, mDataInt.Length + valuesToAdd.Length);
                }

                Array.Copy(valuesToAdd, 0, mDataInt, mPointByPointFillCount - 1, valuesToAdd.Length);
                mPointByPointFillCount += valuesToAdd.Length;

                //for (var index = 0; index < valuesToAdd.Length; index++)
                //{
                //    mDataInt[mPointByPointFillCount] = valuesToAdd[index];
                //    mPointByPointFillCount++;
                //}
            }
            catch
            {
                success = false;
            }

            return success;
        }

        public bool FillWithDataAddBlock(float[] valuesToAdd)
        {
            var success = true;

            try
            {
                if (mDataSingle.Length <= mPointByPointFillCount + valuesToAdd.Length - 1)
                {
                    Array.Resize(ref mDataSingle, mDataSingle.Length + valuesToAdd.Length);
                }

                Array.Copy(valuesToAdd, 0, mDataSingle, mPointByPointFillCount - 1, valuesToAdd.Length);
                mPointByPointFillCount += valuesToAdd.Length;
            }
            catch
            {
                success = false;
            }

            return success;
        }

        public bool FillWithDataAddBlock(double[] valuesToAdd)
        {
            var success = true;

            try
            {
                if (mDataDouble.Length <= mPointByPointFillCount + valuesToAdd.Length - 1)
                {
                    Array.Resize(ref mDataDouble, mDataDouble.Length + valuesToAdd.Length);
                }

                Array.Copy(valuesToAdd, 0, mDataDouble, mPointByPointFillCount, valuesToAdd.Length);
                mPointByPointFillCount += valuesToAdd.Length;
            }
            catch
            {
                success = false;
            }

            return success;
        }

        public bool FillWithDataAddPoint(int valueToAdd)
        {
            var success = true;

            try
            {
                if (mDataType != DataTypeToUse.FillingIntegerType)
                {
                    switch (mDataType)
                    {
                        case DataTypeToUse.FillingSingleType:
                            success = FillWithDataAddPoint((float)valueToAdd);
                            break;
                        case DataTypeToUse.FillingDoubleType:
                            success = FillWithDataAddPoint((double)valueToAdd);
                            break;
                        default:
                            success = false;
                            break;
                    }
                }
                else
                {
                    if (mDataInt.Length <= mPointByPointFillCount)
                    {
                        Array.Resize(ref mDataInt, (int)Math.Round(mDataInt.Length * 1.1d));
                    }

                    mDataInt[mPointByPointFillCount] = valueToAdd;
                    mPointByPointFillCount += 1;
                }
            }
            catch
            {
                success = false;
            }

            return success;
        }

        public bool FillWithDataAddPoint(float valueToAdd)
        {
            var success = true;

            try
            {
                if (mDataType != DataTypeToUse.FillingSingleType)
                {
                    switch (mDataType)
                    {
                        case DataTypeToUse.FillingIntegerType:
                            success = FillWithDataAddPoint((int)Math.Round(valueToAdd));
                            break;
                        case DataTypeToUse.FillingDoubleType:
                            success = FillWithDataAddPoint((double)valueToAdd);
                            break;
                        default:
                            success = false;
                            break;
                    }
                }
                else
                {
                    if (mDataSingle.Length <= mPointByPointFillCount)
                    {
                        Array.Resize(ref mDataSingle, (int)Math.Round(mDataSingle.Length * 1.1d));
                    }

                    mDataSingle[mPointByPointFillCount] = valueToAdd;
                    mPointByPointFillCount += 1;
                }
            }
            catch
            {
                success = false;
            }

            return success;
        }

        public bool FillWithDataAddPoint(double valueToAdd)
        {
            var success = true;

            try
            {
                if (mDataType != DataTypeToUse.FillingDoubleType)
                {
                    switch (mDataType)
                    {
                        case DataTypeToUse.FillingIntegerType:
                            success = FillWithDataAddPoint((int)Math.Round(valueToAdd));
                            break;
                        case DataTypeToUse.FillingSingleType:
                            success = FillWithDataAddPoint((float)valueToAdd);
                            break;
                        default:
                            success = false;
                            break;
                    }
                }
                else
                {
                    if (mDataDouble.Length <= mPointByPointFillCount)
                    {
                        Array.Resize(ref mDataDouble, (int)Math.Round(mDataDouble.Length * 1.1d));
                    }

                    mDataDouble[mPointByPointFillCount] = valueToAdd;
                    mPointByPointFillCount += 1;
                }
            }
            catch
            {
                success = false;
            }

            return success;
        }

        public bool FinalizeDataFill()
        {
            // Finalizes point-by-point data filling
            // Call this after calling FillWithDataAddPoint with each point

            Array DataArray = null;
            bool success;

            try
            {
                success = true;
                switch (mDataType)
                {
                    case DataTypeToUse.FillingIntegerType:
                        mDataType = DataTypeToUse.IntegerType;

                        // Shrink mDataInt if necessary
                        if (mDataInt.Length > mPointByPointFillCount)
                        {
                            Array.Resize(ref mDataInt, mPointByPointFillCount);
                        }

                        DataArray = mDataInt;
                        break;

                    case DataTypeToUse.FillingSingleType:
                        mDataType = DataTypeToUse.SingleType;

                        // Shrink mDataSingle if necessary
                        if (mDataSingle.Length > mPointByPointFillCount)
                        {
                            Array.Resize(ref mDataSingle, mPointByPointFillCount);
                        }

                        DataArray = mDataSingle;
                        break;

                    case DataTypeToUse.FillingDoubleType:
                        mDataType = DataTypeToUse.DoubleType;

                        // Shrink mDataDouble if necessary
                        if (mDataDouble.Length > mPointByPointFillCount)
                        {
                            Array.Resize(ref mDataDouble, mPointByPointFillCount);
                        }

                        DataArray = mDataDouble;
                        break;

                    default:
                        // Not filling
                        success = false;
                        break;
                }

                if (success && DataArray != null)
                {
                    if (UsePointerIndexArray)
                    {
                        InitializePointerIndexArray(DataArray.Length);
                        Array.Sort(DataArray, mPointerIndices);
                    }
                    else
                    {
                        Array.Sort(DataArray);
                        mPointerIndices = new int[0];
                        mPointerArrayIsValid = false;
                    }
                }
            }
            catch
            {
                success = false;
            }

            return success;
        }

        public bool FindValueRange(int searchValue, int toleranceHalfWidth, ref int matchIndexStart, ref int matchIndexEnd)
        {
            // Searches the loaded data for searchValue with a tolerance of +-tolerance
            // Returns True if a match is found; in addition, populates matchIndexStart and matchIndexEnd
            // Otherwise, returns false

            bool matchFound;

            // See if user filled with data, but didn't call Finalize
            // We'll call it for them
            if (mDataType == DataTypeToUse.FillingIntegerType || mDataType == DataTypeToUse.FillingSingleType || mDataType == DataTypeToUse.FillingDoubleType)
            {
                FinalizeDataFill();
            }

            if (mDataType != DataTypeToUse.IntegerType)
            {
                switch (mDataType)
                {
                    case DataTypeToUse.SingleType:
                        matchFound = FindValueRange(searchValue, (float)toleranceHalfWidth, ref matchIndexStart, ref matchIndexEnd);
                        break;
                    case DataTypeToUse.DoubleType:
                        matchFound = FindValueRange(searchValue, (double)toleranceHalfWidth, ref matchIndexStart, ref matchIndexEnd);
                        break;
                    default:
                        matchFound = false;
                        break;
                }
            }
            else
            {
                matchIndexStart = 0;
                matchIndexEnd = mDataInt.Length - 1;

                if (mDataInt.Length == 0)
                {
                    matchIndexEnd = -1;
                }
                else if (mDataInt.Length == 1)
                {
                    if (Math.Abs(searchValue - mDataInt[0]) > toleranceHalfWidth)
                    {
                        // Only one data point, and it is not within tolerance
                        matchIndexEnd = -1;
                    }
                }
                else
                {
                    BinarySearchRangeInt(searchValue, toleranceHalfWidth, ref matchIndexStart, ref matchIndexEnd);
                }

                if (matchIndexStart > matchIndexEnd)
                {
                    matchIndexStart = -1;
                    matchIndexEnd = -1;
                    matchFound = false;
                }
                else
                {
                    matchFound = true;
                }
            }

            return matchFound;
        }

        public bool FindValueRange(double searchValue, double toleranceHalfWidth, ref int matchIndexStart, ref int matchIndexEnd)
        {
            // Searches the loaded data for searchValue with a tolerance of +-tolerance
            // Returns True if a match is found; in addition, populates matchIndexStart and matchIndexEnd
            // Otherwise, returns false

            bool matchFound;

            // See if user filled with data, but didn't call Finalize
            // We'll call it for them
            if (mDataType == DataTypeToUse.FillingIntegerType || mDataType == DataTypeToUse.FillingSingleType || mDataType == DataTypeToUse.FillingDoubleType)
            {
                FinalizeDataFill();
            }

            if (mDataType != DataTypeToUse.DoubleType)
            {
                switch (mDataType)
                {
                    case DataTypeToUse.IntegerType:
                        matchFound = FindValueRange((int)Math.Round(searchValue), (int)Math.Round(toleranceHalfWidth), ref matchIndexStart, ref matchIndexEnd);
                        break;
                    case DataTypeToUse.SingleType:
                        matchFound = FindValueRange((float)searchValue, (float)toleranceHalfWidth, ref matchIndexStart, ref matchIndexEnd);
                        break;
                    default:
                        matchFound = false;
                        break;
                }
            }
            else
            {
                matchIndexStart = 0;
                matchIndexEnd = mDataDouble.Length - 1;

                if (mDataDouble.Length == 0)
                {
                    matchIndexEnd = -1;
                }
                else if (mDataDouble.Length == 1)
                {
                    if (Math.Abs(searchValue - mDataDouble[0]) > toleranceHalfWidth)
                    {
                        // Only one data point, and it is not within tolerance
                        matchIndexEnd = -1;
                    }
                }
                else
                {
                    BinarySearchRangeDbl(searchValue, toleranceHalfWidth, ref matchIndexStart, ref matchIndexEnd);
                }

                if (matchIndexStart > matchIndexEnd)
                {
                    matchIndexStart = -1;
                    matchIndexEnd = -1;
                    matchFound = false;
                }
                else
                {
                    matchFound = true;
                }
            }

            return matchFound;
        }

        public bool FindValueRange(float searchValue, float toleranceHalfWidth, ref int matchIndexStart, ref int matchIndexEnd)
        {
            // Searches the loaded data for searchValue with a tolerance of +-tolerance
            // Returns True if a match is found; in addition, populates matchIndexStart and matchIndexEnd
            // Otherwise, returns false

            bool matchFound;

            // See if user filled with data, but didn't call Finalize
            // We'll call it for them
            if (mDataType == DataTypeToUse.FillingIntegerType || mDataType == DataTypeToUse.FillingSingleType || mDataType == DataTypeToUse.FillingDoubleType)
            {
                FinalizeDataFill();
            }

            if (mDataType != DataTypeToUse.SingleType)
            {
                switch (mDataType)
                {
                    case DataTypeToUse.IntegerType:
                        matchFound = FindValueRange((int)Math.Round(searchValue), (int)Math.Round(toleranceHalfWidth), ref matchIndexStart, ref matchIndexEnd);
                        break;
                    case DataTypeToUse.DoubleType:
                        matchFound = FindValueRange(searchValue, (double)toleranceHalfWidth, ref matchIndexStart, ref matchIndexEnd);
                        break;
                    default:
                        matchFound = false;
                        break;
                }
            }
            else
            {
                matchIndexStart = 0;
                matchIndexEnd = mDataSingle.Length - 1;
                if (mDataSingle.Length == 0)
                {
                    matchIndexEnd = -1;
                }
                else if (mDataSingle.Length == 1)
                {
                    if (Math.Abs(searchValue - mDataSingle[0]) > toleranceHalfWidth)
                    {
                        // Only one data point, and it is not within tolerance
                        matchIndexEnd = -1;
                    }
                }
                else
                {
                    BinarySearchRangeSng(searchValue, toleranceHalfWidth, ref matchIndexStart, ref matchIndexEnd);
                }

                if (matchIndexStart > matchIndexEnd)
                {
                    matchIndexStart = -1;
                    matchIndexEnd = -1;
                    matchFound = false;
                }
                else
                {
                    matchFound = true;
                }
            }

            return matchFound;
        }

        public int GetValueByIndexInt(int index)
        {
            try
            {
                return (int)Math.Round(GetValueByIndex(index));
            }
            catch
            {
                return 0;
            }
        }

        public double GetValueByIndex(int index)
        {
            try
            {
                if (mDataType == DataTypeToUse.NoDataPresent)
                {
                    return 0d;
                }

                switch (mDataType)
                {
                    case DataTypeToUse.IntegerType:
                    case DataTypeToUse.FillingIntegerType:
                        return mDataInt[index];
                    case DataTypeToUse.SingleType:
                    case DataTypeToUse.FillingSingleType:
                        return mDataSingle[index];
                    case DataTypeToUse.DoubleType:
                    case DataTypeToUse.FillingDoubleType:
                        return mDataDouble[index];
                }
            }
            catch
            {
                // index is probably out of range
                return 0d;
            }

            return 0d;
        }

        public float GetValueByIndexSng(int index)
        {
            try
            {
                return (float)GetValueByIndex(index);
            }
            catch
            {
                return 0f;
            }
        }

        public int GetValueByOriginalIndexInt(int index)
        {
            try
            {
                return (int)Math.Round(GetValueByOriginalIndex(index));
            }
            catch
            {
                return 0;
            }
        }

        public double GetValueByOriginalIndex(int indexOriginal)
        {
            if (!mPointerArrayIsValid || mDataType == DataTypeToUse.NoDataPresent)
            {
                return 0d;
            }

            try
            {
                var index = Array.IndexOf(mPointerIndices, indexOriginal);
                if (index >= 0)
                {
                    switch (mDataType)
                    {
                        case DataTypeToUse.IntegerType:
                            return mDataInt[mPointerIndices[index]];
                        case DataTypeToUse.SingleType:
                            return mDataSingle[mPointerIndices[index]];
                        case DataTypeToUse.DoubleType:
                            return mDataDouble[mPointerIndices[index]];
                    }
                }
                else
                {
                    return 0d;
                }
            }
            catch
            {
                return 0d;
            }

            return 0d;
        }

        public float GetValueByOriginalIndexSng(int index)
        {
            try
            {
                return (float)GetValueByOriginalIndex(index);
            }
            catch
            {
                return 0f;
            }
        }

        private void InitializeLocalVariables()
        {
            mDataType = DataTypeToUse.NoDataPresent;
            ClearUnusedData();

            UsePointerIndexArray = true;
            InitializePointerIndexArray(0);
        }

        private void InitializePointerIndexArray(int length)
        {
            int index;

            if (length < 0)
            {
                length = 0;
            }

            mPointerIndices = new int[length];

            for (index = 0; index < length; index++)
            {
                mPointerIndices[index] = index;
            }

            if (length > 0)
            {
                mPointerArrayIsValid = true;
            }
            else
            {
                mPointerArrayIsValid = false;
            }
        }
    }
}